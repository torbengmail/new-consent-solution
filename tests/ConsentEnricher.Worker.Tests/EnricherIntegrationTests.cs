using System.Text;
using System.Text.Json;
using TestUtilities;

namespace ConsentEnricher.Worker.Tests;

/// <summary>
/// Integration tests for the Consent Enricher Worker.
/// Uses Testcontainers PostgreSQL (with seeded data) and GCP Pub/Sub emulator.
/// Simulates the Pub/Sub push delivery by POSTing to the /enrich endpoint.
/// </summary>
public class EnricherIntegrationTests : IClassFixture<CustomEnricherFactory>
{
    private readonly CustomEnricherFactory _factory;
    private readonly HttpClient _client;

    public EnricherIntegrationTests(CustomEnricherFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Ping_NoAuth_ReturnsPong()
    {
        var response = await _client.GetAsync("/ping");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("pong", content);
    }

    [Fact]
    public async Task Health_NoAuth_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Enrich_ValidAuditId_PublishesEnrichedMessage()
    {
        var pushMessage = CreatePubSubPushMessage(SeedData.AuditTrailId2);

        var response = await _client.PostAsync("/enrich",
            new StringContent(pushMessage, Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // Poll for messages instead of Task.Delay
        var messages = await PollForEnrichedMessages();

        Assert.True(messages.Count > 0, "Expected at least one enriched Pub/Sub message");

        var data = messages[0].Message.Data.ToStringUtf8();
        var enriched = JsonSerializer.Deserialize<JsonElement>(data);

        Assert.True(enriched.TryGetProperty("user_id", out var userId),
            "Enriched message should contain 'user_id'");
        Assert.Equal(SeedData.TestUserId, userId.GetString());

        Assert.True(enriched.TryGetProperty("is_agreed", out var isAgreed),
            "Enriched message should contain 'is_agreed'");
        Assert.True(isAgreed.GetBoolean(), "Expected is_agreed=true");

        Assert.True(enriched.TryGetProperty("consent_expression_id", out _),
            "Enriched message should contain 'consent_expression_id'");
        Assert.True(enriched.TryGetProperty("consent_id", out _),
            "Enriched message should contain 'consent_id'");
        Assert.True(enriched.TryGetProperty("owner_id", out _),
            "Enriched message should contain 'owner_id'");

        await _factory.AcknowledgeMessages(messages);
    }

    [Fact]
    public async Task Enrich_NonExistentAuditId_Returns200AndPublishesNothing()
    {
        var pushMessage = CreatePubSubPushMessage(999999L);

        var response = await _client.PostAsync("/enrich",
            new StringContent(pushMessage, Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        // Brief wait then verify no messages were published
        await Task.Delay(300);
        var messages = await _factory.PullEnrichedMessages();
        Assert.Empty(messages);
    }

    [Fact]
    public async Task Enrich_MissingData_ReturnsBadRequest()
    {
        var body = JsonSerializer.Serialize(new { message = new { data = (string?)null } });

        var response = await _client.PostAsync("/enrich",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<List<Google.Cloud.PubSub.V1.ReceivedMessage>> PollForEnrichedMessages(
        int intervalMs = 100, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        List<Google.Cloud.PubSub.V1.ReceivedMessage> messages = [];

        while (DateTime.UtcNow < deadline)
        {
            messages = await _factory.PullEnrichedMessages();
            if (messages.Count > 0)
                return messages;
            await Task.Delay(intervalMs);
        }

        return messages;
    }

    private static string CreatePubSubPushMessage(long decisionAuditId)
    {
        var payload = JsonSerializer.Serialize(new { decision_audit_id = decisionAuditId });
        var base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

        return JsonSerializer.Serialize(new
        {
            message = new
            {
                data = base64Data,
                messageId = Guid.NewGuid().ToString(),
                attributes = new Dictionary<string, string>()
            },
            subscription = "projects/test-project/subscriptions/consent-decisions-raw-push"
        });
    }
}
