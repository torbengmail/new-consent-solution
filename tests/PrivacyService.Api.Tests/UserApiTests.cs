using System.Net;
using System.Text.Json;
using TestUtilities;

namespace PrivacyService.Api.Tests;

/// <summary>
/// Tests for the UserApi endpoints. These tests use Basic Auth to authenticate with the
/// test server. In production, the UserApi uses JWT/Connect ID tokens for authentication;
/// Basic Auth is enabled in the test configuration as a testing convenience.
/// </summary>
[Collection("Api")]
public class UserApiTests
{
    private readonly HttpClient _client;

    public UserApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private HttpRequestMessage AuthGet(string url) =>
        ApiTestHelper.AuthGet(url, SeedData.AdminUsername, SeedData.AdminPassword);

    private HttpRequestMessage AuthPost(string url, object? body = null) =>
        ApiTestHelper.AuthPost(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthPut(string url, object body) =>
        ApiTestHelper.AuthPut(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    // ── Auth ────────────────────────────────────────────────────────

    [Fact]
    public async Task UserApi_NoAuth_Returns401()
    {
        var response = await _client.GetAsync("/v1/userapi/languages");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Languages ───────────────────────────────────────────────────

    [Fact]
    public async Task GetLanguages_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/languages"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one language");
    }

    // ── UI Settings ─────────────────────────────────────────────────

    [Fact]
    public async Task GetUiSettings_WithLanguage_ReturnsTextsAndTheme()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/ui-settings?language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("texts", out _), "Response should contain 'texts' property");
        Assert.True(doc.TryGetProperty("theme", out _), "Response should contain 'theme' property");
    }

    // ── DSR Requests ────────────────────────────────────────────────

    [Fact]
    public async Task GetDsrRequests_Authenticated_Returns200()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/dsr-requests"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── DSR Texts ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDsrTexts_WithLanguage_Returns200()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/dsr-texts?language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Legal Texts ─────────────────────────────────────────────────

    [Fact]
    public async Task GetLegalTexts_WithLanguage_Returns200()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/legal-texts?language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Decisions List Grouped ───────────────────────────────────────

    [Fact]
    public async Task PostDecisionsListGrouped_WithOwnerAndLanguage_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            $"/v1/userapi/dashboard/decisions-list-grouped?owner_id={SeedData.DefaultOwnerId}&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Get Random Expressions ──────────────────────────────────────

    [Fact]
    public async Task PostGetRandomExpressions_WithFullParams_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            $"/v1/userapi/user-consent-decisions?owner_id={SeedData.DefaultOwnerId}&product_id={SeedData.CaptureProductId}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Save Decisions ──────────────────────────────────────────────

    [Fact]
    public async Task PutSaveDecisions_EmptyList_ReturnsBadRequest()
    {
        var decisions = Array.Empty<object>();
        var response = await _client.SendAsync(AuthPut("/v1/userapi/user-consent-decisions", decisions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutSaveDecisions_ValidDecision_Returns200()
    {
        var decisions = new[]
        {
            new
            {
                consent_expression_id = SeedData.ConsentExpressionId111,
                is_agreed = true,
                user_consent_source_id = SeedData.UserConsentSourceId,
                presented_language = "en"
            }
        };

        var response = await _client.SendAsync(AuthPut("/v1/userapi/user-consent-decisions", decisions));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Personal Data Dump Links ────────────────────────────────────

    [Fact]
    public async Task GetPersonalDataDumpLinks_WithLanguage_Returns200()
    {
        var response = await _client.SendAsync(AuthGet("/v1/userapi/pd-dump-links-extended?language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
