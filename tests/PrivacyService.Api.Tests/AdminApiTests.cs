using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TestUtilities;

namespace PrivacyService.Api.Tests;

[Collection("Api")]
public class AdminApiTests
{
    private readonly HttpClient _client;

    public AdminApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private HttpRequestMessage AuthGet(string url) =>
        ApiTestHelper.AuthGet(url, SeedData.AdminUsername, SeedData.AdminPassword);

    private HttpRequestMessage AuthPost(string url, object body) =>
        ApiTestHelper.AuthPost(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthPut(string url, object body) =>
        ApiTestHelper.AuthPut(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthDelete(string url) =>
        ApiTestHelper.AuthDelete(url, SeedData.AdminUsername, SeedData.AdminPassword);

    // ── Auth requirement ────────────────────────────────────────────

    [Fact]
    public async Task AdminApi_NoAuth_Returns401()
    {
        var response = await _client.GetAsync("/v1/adminapi/consents");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminApi_InvalidCredentials_Returns401()
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:wrongpassword"));
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/adminapi/consents");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Permission checks (admin lacks READ_USER / READ_USER_REFERENCE_DATA) ──

    [Fact]
    public async Task GetUsers_AdminLacksPermission_Returns403()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/users"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersReferenceData_AdminLacksPermission_Returns403()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/users/reference-data"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ── Read endpoints (admin has these permissions) ─────────────────

    [Fact]
    public async Task GetConsents_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/consents"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one consent in the response array");
    }

    [Fact]
    public async Task GetConsentById_ExistingConsent_ReturnsConsentWithName()
    {
        var response = await _client.SendAsync(AuthGet($"/v1/adminapi/consents/{SeedData.ConsentId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("name", out var name), "Response should contain 'name' property");
        Assert.False(string.IsNullOrEmpty(name.GetString()), "Consent name should not be empty");
    }

    [Fact]
    public async Task GetConsentById_NonExistent_Returns404()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/consents/999999"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetConsentsReferenceData_Authenticated_ReturnsConsentTypes()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/consents/reference-data"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("consent_types", out _), "Response should contain 'consent_types' property");
    }

    [Fact]
    public async Task GetExpressions_ForExistingConsent_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet($"/v1/adminapi/consents/{SeedData.ConsentId}/expressions"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one expression for consent 201");
    }

    [Fact]
    public async Task GetTexts_WithOwnerAndLanguage_Returns200WithContent()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/adminapi/texts?owner_id={SeedData.DefaultOwnerId}&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content), "Texts response should not be empty");
    }

    [Fact]
    public async Task GetTags_Authenticated_Returns200()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/tags"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() >= 0, "Tags array should be a valid (possibly empty) array");
    }

    // ── Write operations ────────────────────────────────────────────

    [Fact]
    public async Task TagLifecycle_CreateThenDelete_Returns201Then204()
    {
        var body = new { name = "integration-test-tag", owner_id = SeedData.DefaultOwnerId };
        var createResponse = await _client.SendAsync(AuthPost("/v1/adminapi/tags", body));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(createResponse);
        Assert.True(doc.TryGetProperty("id", out var idProp), "Created tag should have 'id' property");
        var tagId = idProp.GetInt32();
        Assert.True(tagId > 0, "Tag ID should be positive");

        // Verify the tag exists
        var getResponse = await _client.SendAsync(AuthGet("/v1/adminapi/tags"));
        var tags = await ApiTestHelper.ReadJsonAsync(getResponse);
        var found = false;
        foreach (var tag in tags.EnumerateArray())
        {
            if (tag.TryGetProperty("id", out var tid) && tid.GetInt32() == tagId)
            {
                found = true;
                break;
            }
        }
        Assert.True(found, $"Tag {tagId} should appear in the tags list after creation");

        // Delete it
        var deleteResponse = await _client.SendAsync(
            AuthDelete($"/v1/adminapi/tags?id={tagId}&owner_id={SeedData.DefaultOwnerId}"));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateConsent_ValidPayload_ReturnsNewConsentWithId()
    {
        var body = new
        {
            name = "integration-test-consent",
            description = "test consent for integration",
            owner_id = SeedData.DefaultOwnerId,
            consent_type_id = 1,
            hide_by_default = false,
            is_group = false
        };
        var response = await _client.SendAsync(AuthPost("/v1/adminapi/consents", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("id", out var idProp), "Created consent should have 'id' property");
        var newId = idProp.GetInt32();
        Assert.True(newId > 0, "Consent ID should be positive");

        // Read back
        var getResponse = await _client.SendAsync(AuthGet($"/v1/adminapi/consents/{newId}"));
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var getDoc = await ApiTestHelper.ReadJsonAsync(getResponse);
        Assert.Equal("integration-test-consent", getDoc.GetProperty("name").GetString());
    }

    [Fact]
    public async Task CreateExpression_ValidPayload_ReturnsNewExpressionWithId()
    {
        var body = new
        {
            name = "integration-test-expression",
            description = "test expression",
            status_id = 1,
            is_default = false,
            tag_ids = Array.Empty<int>(),
            texts = new[]
            {
                new { language = "en", title = "Test Title", short_text = "Short", long_text = "Long text" }
            }
        };
        var response = await _client.SendAsync(
            AuthPost($"/v1/adminapi/consents/{SeedData.ConsentId}/expressions", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("id", out var idProp), "Created expression should have 'id' property");
        var newId = idProp.GetInt32();
        Assert.True(newId > 0, "Expression ID should be positive");

        // Read back
        var getResponse = await _client.SendAsync(AuthGet($"/v1/adminapi/expressions/{newId}"));
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var getDoc = await ApiTestHelper.ReadJsonAsync(getResponse);
        Assert.True(getDoc.TryGetProperty("id", out _), "Read-back expression should have 'id' property");
    }

    // ── Expression read endpoints ───────────────────────────────────

    [Fact]
    public async Task GetExpression_ExistingSeedExpression_ReturnsExpressionWithId()
    {
        var response = await _client.SendAsync(AuthGet($"/v1/adminapi/expressions/{SeedData.ExpressionId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("id", out _), "Expression should have 'id' property");
    }

    [Fact]
    public async Task GetExpressionById_NonExistent_Returns404()
    {
        var response = await _client.SendAsync(AuthGet("/v1/adminapi/expressions/999999"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetExpressionTexts_ExistingExpression_Returns200()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/adminapi/expressions/{SeedData.ExpressionId}/texts"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content), "Expression texts response should not be empty");
    }

    // ── Update consent ──────────────────────────────────────────────

    [Fact]
    public async Task UpdateConsent_ExistingConsent_ReturnsUpdatedName()
    {
        // Create a consent to update (test isolation)
        var createBody = new
        {
            name = "consent-to-update",
            description = "original",
            owner_id = SeedData.DefaultOwnerId,
            consent_type_id = 1,
            hide_by_default = false,
            is_group = false
        };
        var createResp = await _client.SendAsync(AuthPost("/v1/adminapi/consents", createBody));
        var created = await ApiTestHelper.ReadJsonAsync(createResp);
        var consentId = created.GetProperty("id").GetInt32();

        // Update it
        var updateBody = new
        {
            name = "consent-updated",
            description = "updated description",
            owner_id = SeedData.DefaultOwnerId,
            consent_type_id = 1,
            hide_by_default = true,
            is_group = false
        };
        var response = await _client.SendAsync(AuthPut($"/v1/adminapi/consents/{consentId}", updateBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.Equal("consent-updated", doc.GetProperty("name").GetString());
    }

    [Fact]
    public async Task UpdateConsent_NonExistent_Returns404()
    {
        var updateBody = new
        {
            name = "ghost-consent",
            description = "does not exist",
            owner_id = SeedData.DefaultOwnerId,
            consent_type_id = 1,
            hide_by_default = false,
            is_group = false
        };
        var response = await _client.SendAsync(AuthPut("/v1/adminapi/consents/999999", updateBody));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Delete consent (soft delete) ────────────────────────────────

    [Fact]
    public async Task DeleteConsent_ExistingConsent_Returns204AndGoneOn404()
    {
        // Create a consent to delete (test isolation)
        var createBody = new
        {
            name = "consent-to-delete",
            description = "will be deleted",
            owner_id = SeedData.DefaultOwnerId,
            consent_type_id = 1,
            hide_by_default = false,
            is_group = false
        };
        var createResp = await _client.SendAsync(AuthPost("/v1/adminapi/consents", createBody));
        var created = await ApiTestHelper.ReadJsonAsync(createResp);
        var consentId = created.GetProperty("id").GetInt32();

        // Delete it
        var response = await _client.SendAsync(AuthDelete($"/v1/adminapi/consents/{consentId}"));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getResponse = await _client.SendAsync(AuthGet($"/v1/adminapi/consents/{consentId}"));
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteConsent_NonExistent_Returns404()
    {
        var response = await _client.SendAsync(AuthDelete("/v1/adminapi/consents/999999"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Update expression ───────────────────────────────────────────

    [Fact]
    public async Task UpdateExpression_NewlyCreatedExpression_ReturnsUpdatedName()
    {
        // Create a fresh expression to update (test isolation — don't mutate seed data)
        var createBody = new
        {
            name = "expression-to-update",
            description = "original",
            status_id = 1,
            is_default = false,
            tag_ids = Array.Empty<int>(),
            texts = new[]
            {
                new { language = "en", title = "Original Title", short_text = "Short", long_text = "Long" }
            }
        };
        var createResp = await _client.SendAsync(
            AuthPost($"/v1/adminapi/consents/{SeedData.ConsentId}/expressions", createBody));
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createdDoc = await ApiTestHelper.ReadJsonAsync(createResp);
        var exprId = createdDoc.GetProperty("id").GetInt32();

        // Update it
        var updateBody = new
        {
            name = "updated-expression",
            description = "updated description",
            consent_id = SeedData.ConsentId,
            status_id = 1,
            is_default = false
        };
        var response = await _client.SendAsync(AuthPut($"/v1/adminapi/expressions/{exprId}", updateBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.Equal("updated-expression", doc.GetProperty("name").GetString());
    }

    // ── Expression text CRUD ────────────────────────────────────────

    [Fact]
    public async Task CreateExpressionText_ValidPayload_Returns200()
    {
        var body = new
        {
            language = "no",
            title = "Test Tittel",
            short_text = "Kort tekst",
            long_text = "Lang tekst for testing"
        };
        var response = await _client.SendAsync(
            AuthPost($"/v1/adminapi/expressions/{SeedData.ExpressionId}/texts", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExpressionText_ExistingText_Returns200()
    {
        // First ensure a text exists
        var createBody = new
        {
            language = "sv",
            title = "Original",
            short_text = "Original short",
            long_text = "Original long"
        };
        await _client.SendAsync(
            AuthPost($"/v1/adminapi/expressions/{SeedData.ExpressionId}/texts", createBody));

        // Update it
        var updateBody = new
        {
            language = "sv",
            title = "Updated Title",
            short_text = "Updated short",
            long_text = "Updated long"
        };
        var response = await _client.SendAsync(
            AuthPut($"/v1/adminapi/expressions/{SeedData.ExpressionId}/texts/sv", updateBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Text field management ───────────────────────────────────────

    [Fact]
    public async Task CreateTextField_ValidPayload_Returns200()
    {
        var body = new
        {
            owner_id = SeedData.DefaultOwnerId,
            language = "en",
            page = "test_page",
            key = "test_key",
            value = "Test value"
        };
        var response = await _client.SendAsync(AuthPost("/v1/adminapi/texts/field", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateTextField_ExistingField_ReturnsUpdatedValue()
    {
        var body = new
        {
            owner_id = SeedData.DefaultOwnerId,
            language = "en",
            page = "test_page",
            key = "updated_key",
            value = "Updated value"
        };
        var response = await _client.SendAsync(AuthPut("/v1/adminapi/texts/field", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.Equal("Updated value", doc.GetProperty("value").GetString());
    }
}
