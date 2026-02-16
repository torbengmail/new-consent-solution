namespace PrivacyConsent.Data.Queries;

public interface ICacheQueries
{
    Task<string?> GetCacheValue(string userId, int idTypeId, string dataKey, int retentionHours);
    Task<List<CacheValueRow>> GetCacheValuesIn(string userId, int idTypeId, IEnumerable<string> dataKeys, int retentionHours);
    Task UpsertCacheValues(string userId, int idTypeId, Dictionary<string, object?> keyValues);
}
