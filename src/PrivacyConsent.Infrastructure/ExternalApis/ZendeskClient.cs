using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrivacyConsent.Domain.Constants;

namespace PrivacyConsent.Infrastructure.ExternalApis;

public class ZendeskClient : IZendeskClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZendeskClient> _logger;

    public ZendeskClient(HttpClient httpClient, IConfiguration config, ILogger<ZendeskClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = config["Zendesk:BaseUrl"] ?? "";
        var username = config["Zendesk:Username"] ?? "";
        var token = config["Zendesk:Token"] ?? "";

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{token}")));
    }

    public async Task<string?> CreateTicketAsync(string subject, string body, string requesterEmail)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("tickets.json", new
            {
                ticket = new
                {
                    subject,
                    description = body,
                    requester = new { email = requesterEmail },
                    tags = new[] { "privacy-service", "dsr" }
                }
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                return result.GetProperty("ticket").GetProperty("id").ToString();
            }

            _logger.LogError("Failed to create Zendesk ticket: {Status}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Zendesk ticket");
            return null;
        }
    }

    public async Task<Dictionary<string, object>> GetRequestStatusesAsync(string email, string ownerTag)
    {
        var result = new Dictionary<string, object>();
        var dsrTypes = DsrConstants.GetDsrTypes(OwnerConstants.TdOwnerId);

        foreach (var type in dsrTypes)
        {
            result[type] = CacheConstants.EmptyValue;
        }

        try
        {
            var response = await _httpClient.GetAsync($"search.json?query=type:ticket requester:{email} tags:dsr");
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                // Parse ticket results and map to DSR types
                // Implementation depends on Zendesk ticket structure
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Zendesk request statuses for {Email}", email);
        }

        return result;
    }

    public async Task<List<ZendeskFileInfo>> GetPersonalDataFilesAsync(string userId)
    {
        // TODO: Zendesk file retrieval API not yet integrated
        _logger.LogWarning("GetPersonalDataFilesAsync not implemented for user {UserId}", userId);
        return [];
    }

    public ZendeskFileSharingResult GetFileSharingLinks(List<ZendeskFileInfo> data)
    {
        return new ZendeskFileSharingResult
        {
            Links = data.Select(d => new ZendeskFileLink
            {
                TicketId = d.TicketId,
                Product = new ZendeskFileProduct { Id = d.ProductId ?? 0, Name = d.ProductName },
                FileName = d.FileName,
                Link = d.Link,
                LastModified = d.LastModified
            }).ToList()
        };
    }
}
