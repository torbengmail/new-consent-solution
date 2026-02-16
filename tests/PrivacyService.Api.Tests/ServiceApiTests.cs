using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TestUtilities;

namespace PrivacyService.Api.Tests;

[Collection("Api")]
public class ServiceApiTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ServiceApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private HttpRequestMessage AuthGet(string url) =>
        ApiTestHelper.AuthGet(url, SeedData.AdminUsername, SeedData.AdminPassword);

    private HttpRequestMessage AuthPost(string url, object? body = null) =>
        ApiTestHelper.AuthPost(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthPut(string url, object body) =>
        ApiTestHelper.AuthPut(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthPatch(string url, object body) =>
        ApiTestHelper.AuthPatch(url, SeedData.AdminUsername, SeedData.AdminPassword, body);

    private HttpRequestMessage AuthDelete(string url) =>
        ApiTestHelper.AuthDelete(url, SeedData.AdminUsername, SeedData.AdminPassword);

    // ── Health & ping ───────────────────────────────────────────────

    [Fact]
    public async Task Ping_NoAuth_ReturnsPong()
    {
        var response = await _client.GetAsync("/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("pong", content);
    }

    [Fact]
    public async Task GetConsents_NoAuth_Returns401()
    {
        var response = await _client.GetAsync($"/v1/serviceapi/consents?owner_id={SeedData.DefaultOwnerId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ServiceApi_InvalidCredentials_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/v1/serviceapi/consents?owner_id={SeedData.DefaultOwnerId}");
        request.Headers.Authorization = ApiTestHelper.BasicAuth("baduser", "badpass");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Dictionary endpoints ────────────────────────────────────────

    [Fact]
    public async Task GetLanguages_Authenticated_ReturnsNonEmptyContent()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/languages"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(content.Length > 2, "Languages response should contain data");
    }

    [Fact]
    public async Task GetOwners_Authenticated_ReturnsNonEmptyContent()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/owners"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.True(content.Length > 2, "Owners response should contain data");
    }

    [Fact]
    public async Task GetConsentTypes_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/consent-types"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one consent type");
    }

    [Fact]
    public async Task GetConsentPurposes_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/consent-purposes"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one consent purpose");
    }

    [Fact]
    public async Task GetExpressionTags_WithOwnerId_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/dictionaries/expression-tags?owner_id={SeedData.DefaultOwnerId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one expression tag");
    }

    [Fact]
    public async Task GetIdTypes_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/id-types"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one ID type");
    }

    [Fact]
    public async Task GetProducts_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/products"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one product");
    }

    [Fact]
    public async Task GetOwnersWithProducts_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/owners-products"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one owner with products");
    }

    [Fact]
    public async Task GetExpressionStatuses_Authenticated_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/expression-statuses"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one expression status");
    }

    // ── Consents ────────────────────────────────────────────────────

    [Fact]
    public async Task GetConsents_WithOwnerId_ReturnsNonEmptyConsentsArray()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/consents?owner_id={SeedData.DefaultOwnerId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("consents", out var arr), "Response should contain 'consents' property");
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one consent for owner 1");
    }

    [Fact]
    public async Task GetConsents_WithUseCaseId_ReturnsFilteredConsents()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/consents?owner_id={SeedData.DefaultOwnerId}&use_case_id={SeedData.UseCaseId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("consents", out var arr), "Response should contain 'consents' property");
        Assert.True(arr.GetArrayLength() > 0, "Expected filtered consents for use case 1001");
    }

    [Fact]
    public async Task GetConsents_MissingOwnerId_Returns400()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/consents"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetConsentById_ExistingConsent_ReturnsConsentWithName()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/consents/{SeedData.ConsentId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("name", out var name), "Response should contain 'name' property");
        Assert.Equal("direct marketing by sms", name.GetString());
    }

    [Fact]
    public async Task GetConsentById_NonExistent_Returns404()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/consents/999999"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Pub/Sub integration ─────────────────────────────────────────

    [Fact]
    public async Task SaveDecisions_ValidPayload_PublishesToPubSubEmulator()
    {
        var decisions = new[]
        {
            new
            {
                consent_expression_id = SeedData.ConsentExpressionId111,
                is_agreed = true,
                user_consent_source_id = SeedData.UserConsentSourceId,
                presented_language = "en",
                user_id = SeedData.TestUserId,
                id_type_id = SeedData.ConnectIdType
            }
        };

        var response = await _client.SendAsync(AuthPut("/v1/serviceapi/user-consent-decisions", decisions));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Poll for messages instead of Task.Delay
        var messages = await ApiTestHelper.PollForPubSubMessages(() => _factory.PullRawMessages());

        Assert.True(messages.Count > 0, "Expected at least one Pub/Sub message after saving decisions");

        var data = messages[0].Message.Data.ToStringUtf8();
        var payload = JsonSerializer.Deserialize<JsonElement>(data);

        Assert.True(payload.TryGetProperty("decision_audit_id", out var auditIdElement),
            "Pub/Sub payload should contain 'decision_audit_id'");
        Assert.True(auditIdElement.GetInt64() > 0, "decision_audit_id should be positive");

        await _factory.AcknowledgeMessages(messages);
    }

    [Fact]
    public async Task SaveDecisions_MalformedBody_Returns400()
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/v1/serviceapi/user-consent-decisions");
        request.Headers.Authorization = ApiTestHelper.BasicAuth(SeedData.AdminUsername, SeedData.AdminPassword);
        request.Content = new StringContent("not-json", Encoding.UTF8, "application/json");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── User consent decisions ──────────────────────────────────────

    [Fact]
    public async Task GetUserConsentDecisionsBatch_WithOwnerId_ReturnsDecisions()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/user-consent-decisions-batch?owner_id={SeedData.DenmarkOwnerId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("decisions", out _), "Response should contain 'decisions' property");
    }

    [Fact]
    public async Task PostUserConsentDecisionsBatchShort_ValidRequest_ReturnsDecisionsWithAgreement()
    {
        var body = new[]
        {
            new { consent_id = SeedData.ConsentExpressionId111, user_id = SeedData.TestUserId2, id_type_id = SeedData.ConnectIdType }
        };

        var response = await _client.SendAsync(AuthPost("/v1/serviceapi/user-consent-decisions-batch", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one batch decision result");
        var first = arr[0];
        Assert.True(first.TryGetProperty("is_agreed", out var isAgreed), "Result should contain 'is_agreed' property");
        Assert.True(isAgreed.GetBoolean(), "Expected is_agreed to be true for test user");
    }

    [Fact]
    public async Task PostUserConsentDecisions_WithFullParams_ReturnsExpressions()
    {
        var response = await _client.SendAsync(AuthPost(
            $"/v1/serviceapi/user-consent-decisions?owner_id={SeedData.DefaultOwnerId}&product_id={SeedData.CaptureProductId}&user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one expression in user consent decisions");
    }

    [Fact]
    public async Task PutUserConsentDecisions_EmptyList_ReturnsBadRequest()
    {
        var decisions = Array.Empty<object>();

        var response = await _client.SendAsync(AuthPut("/v1/serviceapi/user-consent-decisions", decisions));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── Decision history & attempts ─────────────────────────────────

    [Fact]
    public async Task GetUserDecisionHistory_ExistingUser_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(AuthGet(
            $"/v1/serviceapi/user-decision-history?consent_id={SeedData.ConsentExpressionId111}&user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one history entry for user 222");
    }

    [Fact]
    public async Task PostDecisionRequestAttemptEvents_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            $"/v1/serviceapi/decision-request-attempts?consent_id={SeedData.ConsentId}&user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDecisionRequestAttempts_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthGet(
            $"/v1/serviceapi/decision-request-attempts?consent_id={SeedData.ConsentId}&user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Texts & translations ────────────────────────────────────────

    [Fact]
    public async Task GetTexts_MissingOwnerId_Returns400()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/texts?language=en"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTexts_NonExistentLanguage_Returns200EmptyObject()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/texts?owner_id={SeedData.DefaultOwnerId}&product_id=1&language=xx"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("{}", content);
    }

    [Fact]
    public async Task GetConsentExpressionTexts_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthGet(
            $"/v1/serviceapi/texts/consent-expression?consent_id={SeedData.ConsentId}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Data dumps ──────────────────────────────────────────────────

    [Fact]
    public async Task GetUserDataDumpJson_ExistingUser_ReturnsDecisionsAndAttempts()
    {
        var response = await _client.SendAsync(AuthGet(
            $"/v1/serviceapi/user-data-dump-json?user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("decisions", out _), "Response should contain 'decisions' property");
        Assert.True(doc.TryGetProperty("request_attempts", out _), "Response should contain 'request_attempts' property");
    }

    [Fact]
    public async Task GetUserDataDumpCsv_ExistingUser_ReturnsCsvContentType()
    {
        var response = await _client.SendAsync(AuthGet(
            $"/v1/serviceapi/user-data-dump-csv?user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    // ── User consent sources ────────────────────────────────────────

    [Fact]
    public async Task GetUserConsentSources_WithOwnerId_ReturnsNonEmptyArray()
    {
        var response = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/user-consent-sources?owner_id={SeedData.DefaultOwnerId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var arr = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(arr.GetArrayLength() > 0, "Expected at least one user consent source");
    }

    // ── Retract/update decisions ────────────────────────────────────

    [Fact]
    public async Task RetractLastDecision_NonExistentUser_Returns404()
    {
        var body = new
        {
            user_id = "nonexistent_user_xyz",
            user_consent_source_id = SeedData.UserConsentSourceId,
            id_type_id = SeedData.ConnectIdType,
            consent_id = SeedData.ConsentId
        };

        var response = await _client.SendAsync(AuthPatch(
            "/v1/serviceapi/retract-last-user-consent-decision", body));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLastDecision_NonExistentUser_Returns404()
    {
        var body = new
        {
            user_id = "nonexistent_user_update_xyz",
            user_consent_source_id = SeedData.UserConsentSourceId,
            id_type_id = SeedData.ConnectIdType,
            consent_id = SeedData.ConsentId,
            value = true
        };

        var response = await _client.SendAsync(AuthPatch(
            "/v1/serviceapi/update-last-user-consent-decision", body));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Telenor ID DSR endpoints ────────────────────────────────────

    [Fact]
    public async Task GetTelenorIdDsrRequests_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthGet(
            "/v1/serviceapi/telenor-id-dsr/requests?user-id=test-user&email=test@test.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateTelenorIdDsrRequest_ValidParams_ReturnsTicketId()
    {
        var response = await _client.SendAsync(AuthPost(
            "/v1/serviceapi/telenor-id-dsr/requests?user-id=test-dsr-user&email=test@test.com&dsr-type=access"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("ticket_id", out _), "Response should contain 'ticket_id' property");
    }

    [Fact]
    public async Task GetTelenorIdDataDumpLinks_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthGet(
            "/v1/serviceapi/telenor-id-dsr/data-dump-links?user-id=test-user"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Generic DSR endpoints ───────────────────────────────────────

    [Fact]
    public async Task GetDsrRequests_ValidType_Returns200()
    {
        var response = await _client.SendAsync(AuthGet(
            "/v1/serviceapi/dsr/requests?type=export"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateDsrRequest_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            "/v1/serviceapi/dsr/requests?user-id=test-dsr-generic&type=export"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateDsrRequest_ExistingRequest_Returns200()
    {
        // Create first
        await _client.SendAsync(AuthPost(
            "/v1/serviceapi/dsr/requests?user-id=test-dsr-update&ticket-id=upd-ticket&type=export"));

        // Update
        var response = await _client.SendAsync(AuthPatch(
            "/v1/serviceapi/dsr/requests?user-id=test-dsr-update&ticket-id=upd-ticket&type=export&status=done",
            new { }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AutoDeletionRequest_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            "/v1/serviceapi/dsr/auto-deletion-request?user-id=test-auto-del&status=open"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── User consent source CRUD ────────────────────────────────────

    [Fact]
    public async Task CreateUserConsentSource_ValidPayload_Returns201WithId()
    {
        var body = new
        {
            name = "Integration Test Source",
            description = "Test source",
            user_consent_source_type_id = 1,
            owner_id = SeedData.DefaultOwnerId
        };
        var response = await _client.SendAsync(AuthPost("/v1/serviceapi/user-consent-sources", body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("id", out var idProp), "Created source should have 'id' property");
        Assert.True(idProp.GetInt32() > 0, "Source ID should be positive");

        // Read back: verify new source appears in list
        var listResponse = await _client.SendAsync(
            AuthGet($"/v1/serviceapi/user-consent-sources?owner_id={SeedData.DefaultOwnerId}"));
        var list = await ApiTestHelper.ReadJsonAsync(listResponse);
        var found = false;
        foreach (var item in list.EnumerateArray())
        {
            if (item.TryGetProperty("id", out var sid) && sid.GetInt32() == idProp.GetInt32())
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "Newly created source should appear in the sources list");
    }

    [Fact]
    public async Task UpdateUserConsentSource_ExistingSource_ReturnsUpdatedName()
    {
        // Create first
        var createBody = new
        {
            name = "Source To Update",
            description = "Original",
            user_consent_source_type_id = 1,
            owner_id = SeedData.DefaultOwnerId
        };
        var createResponse = await _client.SendAsync(AuthPost("/v1/serviceapi/user-consent-sources", createBody));
        var created = await ApiTestHelper.ReadJsonAsync(createResponse);
        var sourceId = created.GetProperty("id").GetInt32();

        // Update
        var updateBody = new
        {
            id = sourceId,
            name = "Updated Source",
            description = "Updated",
            user_consent_source_type_id = 1,
            owner_id = SeedData.DefaultOwnerId
        };
        var response = await _client.SendAsync(AuthPut("/v1/serviceapi/user-consent-sources", updateBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.Equal("Updated Source", doc.GetProperty("name").GetString());
    }

    [Fact]
    public async Task PatchUserConsentSource_ExistingSource_Returns200()
    {
        // Create first
        var createBody = new
        {
            name = "Source To Patch",
            description = "Original",
            user_consent_source_type_id = 1,
            owner_id = SeedData.DefaultOwnerId
        };
        var createResponse = await _client.SendAsync(AuthPost("/v1/serviceapi/user-consent-sources", createBody));
        var created = await ApiTestHelper.ReadJsonAsync(createResponse);
        var sourceId = created.GetProperty("id").GetInt32();

        // Patch
        var patchBody = new
        {
            id = sourceId,
            name = "Patched Source",
            owner_id = SeedData.DefaultOwnerId
        };
        var response = await _client.SendAsync(AuthPatch("/v1/serviceapi/user-consent-sources", patchBody));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUserConsentSource_ExistingSource_Returns204()
    {
        // Create first
        var createBody = new
        {
            name = "Source To Delete",
            description = "Will be deleted",
            user_consent_source_type_id = 1,
            owner_id = SeedData.DefaultOwnerId
        };
        var createResponse = await _client.SendAsync(AuthPost("/v1/serviceapi/user-consent-sources", createBody));
        var created = await ApiTestHelper.ReadJsonAsync(createResponse);
        var sourceId = created.GetProperty("id").GetInt32();

        // Delete
        var response = await _client.SendAsync(
            AuthDelete($"/v1/serviceapi/user-consent-sources?id={sourceId}&owner_id={SeedData.DefaultOwnerId}"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── Dashboard ───────────────────────────────────────────────────

    [Fact]
    public async Task PostConsentsListGrouped_ValidParams_Returns200()
    {
        var response = await _client.SendAsync(AuthPost(
            $"/v1/serviceapi/dashboard/consents-list-grouped?user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}&owner_id={SeedData.DefaultOwnerId}&expression_tag=privacy-dashboard&language=en"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── File upload ─────────────────────────────────────────────────

    [Fact]
    public async Task GetFileUploadLink_ValidParams_ReturnsS3Link()
    {
        var response = await _client.SendAsync(AuthGet(
            "/v1/serviceapi/file-upload-link?user_id=test-user&product_id=1&file_name=test.txt"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("s3_link", out _), "Response should contain 's3_link' property");
    }

    // ── User info ───────────────────────────────────────────────────

    [Fact]
    public async Task GetUserInfo_Authenticated_ReturnsUsernameAndPermissions()
    {
        var response = await _client.SendAsync(AuthGet("/v1/serviceapi/users/user-info"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("username", out _), "Response should contain 'username' property");
        Assert.True(doc.TryGetProperty("permissions", out _), "Response should contain 'permissions' property");
        Assert.True(doc.TryGetProperty("owners", out _), "Response should contain 'owners' property");
    }

    // ── ID type & mapping ───────────────────────────────────────────

    [Fact]
    public async Task CreateIdType_ValidPayload_ReturnsNewIdType()
    {
        var body = new { name = "integration-test-id-type" };
        var response = await _client.SendAsync(AuthPost("/v1/serviceapi/id-type", body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = await ApiTestHelper.ReadJsonAsync(response);
        Assert.True(doc.TryGetProperty("id", out _), "Created ID type should have 'id' property");

        // Verify it appears in ID types list
        var listResponse = await _client.SendAsync(AuthGet("/v1/serviceapi/dictionaries/id-types"));
        var list = await ApiTestHelper.ReadJsonAsync(listResponse);
        Assert.True(list.GetArrayLength() > 0, "ID types list should contain the new type");
    }

    [Fact]
    public async Task CreateIdMapping_ValidPayload_Returns201()
    {
        // Create a new id_type to avoid duplicate key conflicts
        var typeBody = new { name = "mapping-test-id-type" };
        var typeResponse = await _client.SendAsync(AuthPost("/v1/serviceapi/id-type", typeBody));
        Assert.Equal(HttpStatusCode.OK, typeResponse.StatusCode);
        var typeDoc = await ApiTestHelper.ReadJsonAsync(typeResponse);
        var newIdTypeId = typeDoc.GetProperty("id").GetInt32();

        var body = new { id_type_id = newIdTypeId, name = "integration-test-mapping" };
        var response = await _client.SendAsync(AuthPost("/v1/serviceapi/id-mapping", body));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ── Delete test user ────────────────────────────────────────────

    [Fact]
    public async Task DeleteTestUser_NonExistentGroup_Returns200()
    {
        var response = await _client.SendAsync(
            AuthDelete("/v1/serviceapi/test-user?test_user_group=nonexistent-group"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Authorization checks (unprivileged user) ────────────────────

    [Fact]
    public async Task GetUserDataDumpJson_UnprivilegedUser_Returns403()
    {
        var request = ApiTestHelper.AuthGet(
            $"/v1/serviceapi/user-data-dump-json?user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}",
            SeedData.UnprivilegedUsername, SeedData.UnprivilegedPassword);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserDataDumpCsv_UnprivilegedUser_Returns403()
    {
        var request = ApiTestHelper.AuthGet(
            $"/v1/serviceapi/user-data-dump-csv?user_id={SeedData.TestUserId}&id_type_id={SeedData.ConnectIdType}",
            SeedData.UnprivilegedUsername, SeedData.UnprivilegedPassword);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTestUser_UnprivilegedUser_Returns403()
    {
        var request = ApiTestHelper.AuthDelete(
            "/v1/serviceapi/test-user?test_user_group=nonexistent-group",
            SeedData.UnprivilegedUsername, SeedData.UnprivilegedPassword);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateIdType_UnprivilegedUser_Returns403()
    {
        var request = ApiTestHelper.AuthPost("/v1/serviceapi/id-type",
            SeedData.UnprivilegedUsername, SeedData.UnprivilegedPassword,
            new { name = "test-id-type" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateIdMapping_UnprivilegedUser_Returns403()
    {
        var request = ApiTestHelper.AuthPost("/v1/serviceapi/id-mapping",
            SeedData.UnprivilegedUsername, SeedData.UnprivilegedPassword,
            new { id_type_id = SeedData.ConnectIdType, name = "test-mapping" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
