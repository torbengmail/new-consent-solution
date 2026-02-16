using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PrivacyService.Api.Tests;

/// <summary>
/// Shared helpers for API test authentication and common operations.
/// </summary>
public static class ApiTestHelper
{
    public static AuthenticationHeaderValue BasicAuth(string username, string password)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return new AuthenticationHeaderValue("Basic", encoded);
    }

    public static HttpRequestMessage AuthGet(string url, string username, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = BasicAuth(username, password);
        return request;
    }

    public static HttpRequestMessage AuthPost(string url, string username, string password, object? body = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = BasicAuth(username, password);
        if (body != null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return request;
    }

    public static HttpRequestMessage AuthPut(string url, string username, string password, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        request.Headers.Authorization = BasicAuth(username, password);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return request;
    }

    public static HttpRequestMessage AuthPatch(string url, string username, string password, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url);
        request.Headers.Authorization = BasicAuth(username, password);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        return request;
    }

    public static HttpRequestMessage AuthDelete(string url, string username, string password)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = BasicAuth(username, password);
        return request;
    }

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    /// <summary>
    /// Polls for Pub/Sub messages with retries instead of using Task.Delay.
    /// </summary>
    public static async Task<List<Google.Cloud.PubSub.V1.ReceivedMessage>> PollForPubSubMessages(
        Func<Task<List<Google.Cloud.PubSub.V1.ReceivedMessage>>> pullFunc,
        int intervalMs = 100,
        int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        List<Google.Cloud.PubSub.V1.ReceivedMessage> messages = [];

        while (DateTime.UtcNow < deadline)
        {
            messages = await pullFunc();
            if (messages.Count > 0)
                return messages;
            await Task.Delay(intervalMs);
        }

        return messages;
    }
}
