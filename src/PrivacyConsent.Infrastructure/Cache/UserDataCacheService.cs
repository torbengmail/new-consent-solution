using System.Text.Json;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;

namespace PrivacyConsent.Infrastructure.Cache;

public class UserDataCacheService
{
    private readonly ICacheQueries _cacheQueries;

    public UserDataCacheService(ICacheQueries cacheQueries)
    {
        _cacheQueries = cacheQueries;
    }

    public async Task<T?> GetAndPutIfAbsent<T>(string userId, int idTypeId, string cacheKey,
        Func<Task<Dictionary<string, object?>>> getValueFunction,
        int retentionHours = CacheConstants.DefaultRetentionHours)
    {
        var cachedValue = await _cacheQueries.GetCacheValue(userId, idTypeId, cacheKey, retentionHours);

        if (cachedValue != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        var newValues = await getValueFunction();
        await _cacheQueries.UpsertCacheValues(userId, idTypeId, newValues);

        if (newValues.TryGetValue(cacheKey, out var value) && value != null)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<T>(json);
        }

        return default;
    }

    public async Task PutValues(string userId, int idTypeId, Dictionary<string, object?> keyValues)
    {
        await _cacheQueries.UpsertCacheValues(userId, idTypeId, keyValues);
    }

    public async Task PutValue(string userId, int idTypeId, string cacheKey, object? value)
    {
        await _cacheQueries.UpsertCacheValues(userId, idTypeId, new Dictionary<string, object?> { [cacheKey] = value });
    }

    public async Task<T?> GetValue<T>(string userId, int idTypeId, string cacheKey, int retentionHours)
    {
        var value = await _cacheQueries.GetCacheValue(userId, idTypeId, cacheKey, retentionHours);
        if (value == null) return default;
        return JsonSerializer.Deserialize<T>(value);
    }

    public async Task<List<CacheValueRow>> GetValuesIn(
        string userId, int idTypeId, IEnumerable<string> keys, int retentionHours)
    {
        return await _cacheQueries.GetCacheValuesIn(userId, idTypeId, keys, retentionHours);
    }

    public async Task<Dictionary<int, Dictionary<string, object?>>> GetValuesInByOwner(
        string userId, int idTypeId, IEnumerable<string> keys, int retentionHours)
    {
        var rows = await _cacheQueries.GetCacheValuesIn(userId, idTypeId, keys, retentionHours);
        var result = new Dictionary<int, Dictionary<string, object?>>();

        foreach (var row in rows)
        {
            var (ownerId, key) = CacheConstants.ParseCompositeKey(row.Key);
            if (!result.ContainsKey(ownerId))
                result[ownerId] = new Dictionary<string, object?>();

            result[ownerId][key] = row.Value != null ? JsonSerializer.Deserialize<object>(row.Value) : null;
        }

        return result;
    }
}
