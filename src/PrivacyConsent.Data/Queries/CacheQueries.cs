using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class CacheQueries : ICacheQueries
{
    private readonly PrivacyDbContext _db;

    public CacheQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<string?> GetCacheValue(string userId, int idTypeId, string dataKey, int retentionHours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-retentionHours);

        return await _db.UserDataCaches
            .Where(c => c.UserId == userId
                        && c.IdTypeId == idTypeId
                        && c.DataKey == dataKey
                        && c.ModifiedDate >= cutoff)
            .Select(c => c.DataValue)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CacheValueRow>> GetCacheValuesIn(
        string userId, int idTypeId, IEnumerable<string> dataKeys, int retentionHours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-retentionHours);
        var keyList = dataKeys.ToList();

        return await _db.UserDataCaches
            .Where(c => c.UserId == userId
                        && c.IdTypeId == idTypeId
                        && keyList.Contains(c.DataKey)
                        && c.ModifiedDate >= cutoff)
            .Select(c => new CacheValueRow { Key = c.DataKey, Value = c.DataValue })
            .ToListAsync();
    }

    public async Task UpsertCacheValues(string userId, int idTypeId, Dictionary<string, object?> keyValues)
    {
        foreach (var (key, value) in keyValues)
        {
            var serializedValue = value != null
                ? System.Text.Json.JsonSerializer.Serialize(value)
                : null;

            var existing = await _db.UserDataCaches
                .FirstOrDefaultAsync(c => c.UserId == userId && c.IdTypeId == idTypeId && c.DataKey == key);

            if (existing != null)
            {
                existing.DataValue = serializedValue;
                existing.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                _db.UserDataCaches.Add(new UserDataCache
                {
                    UserId = userId,
                    IdTypeId = idTypeId,
                    DataKey = key,
                    DataValue = serializedValue,
                    ModifiedDate = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
    }

}

public record CacheValueRow
{
    public string Key { get; init; } = "";
    public string? Value { get; init; }
}
