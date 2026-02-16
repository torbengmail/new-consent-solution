using System.Text.Json;
using ConsentEnricher.Worker.Services;
using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Models;
using PrivacyConsent.Infrastructure.PubSub;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<PrivacyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PrivacyDb")));

// Queries
builder.Services.AddScoped<EnricherQueries>();

// Services
builder.Services.Configure<EnrichmentConfig>(builder.Configuration.GetSection("Enrichment"));
builder.Services.AddScoped<EnrichmentService>();

// Pub/Sub (enriched output publisher)
// In production, configure PublisherClient via DI
builder.Services.AddScoped<IEnrichedEventPublisher, PubSubEnrichedEventPublisher>();

// Health check
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

// Ping endpoint (matches original)
app.MapGet("/ping", () => "pong");

// Pub/Sub push endpoint - receives messages from consent-decisions-raw subscription
app.MapPost("/enrich", async (HttpContext context, EnrichmentService enrichmentService, IEnrichedEventPublisher publisher, IConfiguration config, ILogger<Program> logger) =>
{
    // Validate push auth token if configured
    var expectedToken = config["PubSub:PushAuthToken"];
    if (!string.IsNullOrEmpty(expectedToken))
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader == null || !authHeader.StartsWith("Bearer ") ||
            authHeader["Bearer ".Length..] != expectedToken)
        {
            return Results.Unauthorized();
        }
    }

    try
    {
        var body = await JsonSerializer.DeserializeAsync<PubSubPushMessage>(context.Request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (body?.Message?.Data == null)
            return Results.BadRequest("Missing message data");

        var decodedData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(body.Message.Data));
        var payload = JsonSerializer.Deserialize<JsonElement>(decodedData);

        if (!payload.TryGetProperty("decision_audit_id", out var auditIdElement))
            return Results.BadRequest("Missing decision_audit_id");

        var decisionAuditId = auditIdElement.GetInt64();

        logger.LogInformation("Enriching decision audit {AuditId}", decisionAuditId);

        var enriched = await enrichmentService.EnrichConsentValueAsync(decisionAuditId);

        if (enriched != null)
        {
            await publisher.PublishEnrichedAsync(enriched);
            logger.LogInformation("Published enriched consent for audit {AuditId}", decisionAuditId);
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to enrich consent decision");
        return Results.StatusCode(500);
    }
});

app.Run();

// Pub/Sub push message structure
public class PubSubPushMessage
{
    public PubSubPushMessageData? Message { get; set; }
    public string? Subscription { get; set; }
}

public class PubSubPushMessageData
{
    public string? Data { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public string? MessageId { get; set; }
}

// Make Program class accessible for WebApplicationFactory in tests
public partial class Program { }
