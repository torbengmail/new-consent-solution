using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrivacyConsent.Domain.Constants;

namespace PrivacyConsent.Infrastructure.ExternalApis;

public class DenmarkApiClient : IDenmarkApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DenmarkApiClient> _logger;
    private readonly string _baseUrl;

    private static readonly Dictionary<string, string> RequestTypeMapping = new()
    {
        ["deletion"] = "RTBF",
        ["portability"] = "Portability",
        ["export"] = "Insight"
    };

    public DenmarkApiClient(HttpClient httpClient, IConfiguration config, ILogger<DenmarkApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = config["DenmarkApi:BaseUrl"] ?? "https://apigatewaydev.telenor.dk/tndi/";
    }

    public async Task<DenmarkUserInfo?> GetUserInfoAsync(string userId, int idTypeId, string connectIdToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}customer?userId={userId}&idTypeId={idTypeId}");
            request.Headers.Add("connectIdToken", connectIdToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                return new DenmarkUserInfo
                {
                    IsUser = true,
                    MinorFlag = body.TryGetProperty("minorFlagCacheable", out var mc) && mc.GetBoolean()
                        ? body.GetProperty("minorFlag").GetBoolean()
                        : null,
                    NemIdValidated = body.TryGetProperty("nemIdValidated", out var nv) && nv.GetBoolean()
                };
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (body.TryGetProperty("message", out var msg) &&
                    msg.GetString()?.StartsWith("customer is not found") == true)
                {
                    return new DenmarkUserInfo { IsUser = false };
                }
            }

            _logger.LogError("Denmark API call failed for user {UserId}:{IdTypeId}: {Status}",
                userId, idTypeId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Denmark API request failed for user {UserId}:{IdTypeId}", userId, idTypeId);
            return null;
        }
    }

    public async Task<bool> IsUserAsync(string userId, int idTypeId, string connectIdToken)
    {
        var info = await GetUserInfoAsync(userId, idTypeId, connectIdToken);
        return info?.IsUser ?? false;
    }

    public async Task<bool> IsCbbUserAsync(string userId, int idTypeId, string connectIdToken)
    {
        // CBB uses the same Denmark API infrastructure
        var info = await GetUserInfoAsync(userId, idTypeId, connectIdToken);
        return info?.IsUser ?? false;
    }

    public async Task<bool> IsNemIdValidatedAsync(string userId, int idTypeId, string connectIdToken)
    {
        var info = await GetUserInfoAsync(userId, idTypeId, connectIdToken);
        return info?.NemIdValidated ?? false;
    }

    public async Task<List<DenmarkDsrRequest>> GetUserRequestsAsync(
        string userId, int idTypeId, string connectIdToken, string requestType)
    {
        try
        {
            var denmarkType = RequestTypeMapping.GetValueOrDefault(requestType, requestType);
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}privacy/request?userId={userId}&idTypeId={idTypeId}&requestType={denmarkType}");
            request.Headers.Add("connectIdToken", connectIdToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (body.TryGetProperty("status", out var status) && status.GetString() == "success" &&
                    body.TryGetProperty("data", out var data))
                {
                    return JsonSerializer.Deserialize<List<DenmarkDsrRequest>>(data.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                }
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get DSR requests for user {UserId}", userId);
            return [];
        }
    }

    public async Task<string?> CreateUserRequestAsync(
        int idTypeId, string connectIdToken, DenmarkCreateRequestParams createParams)
    {
        if (!RequestTypeMapping.TryGetValue(createParams.Right, out var denmarkType))
            return null;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}privacy/request");
            request.Headers.Add("connectIdToken", connectIdToken);
            request.Content = JsonContent.Create(new
            {
                userId = createParams.UserId,
                idTypeId = idTypeId.ToString(),
                requestType = denmarkType,
                email = createParams.Email
            });

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (body.TryGetProperty("status", out var status) && status.GetString() == "success" &&
                    body.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("id", out var id))
                {
                    return id.GetString();
                }
            }

            _logger.LogError("Failed to create DSR request for user {UserId}: {Status}",
                createParams.UserId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DSR request for user {UserId}", createParams.UserId);
            return null;
        }
    }

    public async Task<Dictionary<string, object>> GetPendingRequestsAsync(
        string userId, int idTypeId, string connectIdToken)
    {
        var result = new Dictionary<string, object>
        {
            ["objection"] = CacheConstants.EmptyValue,
            ["restriction"] = CacheConstants.EmptyValue
        };

        foreach (var type in new[] { "deletion", "portability", "export" })
        {
            var requests = await GetUserRequestsAsync(userId, idTypeId, connectIdToken, type);
            var lastRequest = requests.LastOrDefault();

            if (lastRequest == null || IsFinished(lastRequest))
                result[type] = CacheConstants.EmptyValue;
            else
                result[type] = (object?)lastRequest.Id ?? CacheConstants.EmptyValue;
        }

        return result;
    }

    public async Task<List<DenmarkFinishedRequest>> GetFinishedRequestsAsync(
        string userId, int idTypeId, string connectIdToken)
    {
        var results = new List<DenmarkFinishedRequest>();

        foreach (var type in new[] { "export", "portability" })
        {
            var denmarkType = RequestTypeMapping.GetValueOrDefault(type, type);
            var requests = await GetUserRequestsAsync(userId, idTypeId, connectIdToken, type);

            results.AddRange(requests
                .Where(IsFinished)
                .Select(r => new DenmarkFinishedRequest { Id = r.Id ?? "", Type = denmarkType }));
        }

        return results;
    }

    public DenmarkFileSharingResult GetFileSharingLinks(
        List<DenmarkFinishedRequest> data, string userId, int idTypeId,
        string connectIdToken, string language)
    {
        // For now, generate download links based on the Denmark download URL pattern
        var links = data.Select(d => new DenmarkFileLink
        {
            TicketId = d.Id,
            Product = new DenmarkFileProduct
            {
                Id = OwnerConstants.DenmarkProductId,
                Name = OwnerConstants.DenmarkOwnerName
            },
            FileName = $"request-{d.Id}.zip",
            Link = MakeFileDownloadLink(d.Id, d.Type, language)
        }).ToList();

        return new DenmarkFileSharingResult { Links = links };
    }

    private static string MakeFileDownloadLink(string requestId, string requestType, string language)
    {
        var langPath = language == "da" ? "" : "en/";
        return $"{langPath}mit-telenor/download/?type={requestType}&id={requestId}";
    }

    private static bool IsFinished(DenmarkDsrRequest request)
    {
        return request.Step is "Complete" or "Deleting" &&
               request.StepStatus is "Complete" or "Failed";
    }
}
