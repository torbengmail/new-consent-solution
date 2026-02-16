using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class UserConsentSourceQueries : IUserConsentSourceQueries
{
    private readonly PrivacyDbContext _db;

    public UserConsentSourceQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserConsentSource>> GetSources(int? ownerId = null)
    {
        var query = _db.UserConsentSources.AsQueryable();
        if (ownerId.HasValue)
            query = query.Where(s => s.OwnerId == ownerId);
        return await query.ToListAsync();
    }

    public async Task<UserConsentSource?> GetSourceById(int id) =>
        await _db.UserConsentSources.FindAsync(id);

    public async Task<UserConsentSource> CreateSource(string name, string description,
        int sourceTypeId, int ownerId, int? productId)
    {
        var source = new UserConsentSource
        {
            Name = name,
            Description = description,
            UserConsentSourceTypeId = sourceTypeId,
            OwnerId = ownerId,
            ProductId = productId
        };
        _db.UserConsentSources.Add(source);
        await _db.SaveChangesAsync();
        return source;
    }

    public async Task<UserConsentSource?> UpdateSource(int id, string? name, string? description,
        int? sourceTypeId, int ownerId, int? productId)
    {
        var source = await _db.UserConsentSources.FindAsync(id);
        if (source == null) return null;

        if (name != null) source.Name = name;
        if (description != null) source.Description = description;
        if (sourceTypeId.HasValue) source.UserConsentSourceTypeId = sourceTypeId.Value;
        source.OwnerId = ownerId;
        source.ProductId = productId;

        await _db.SaveChangesAsync();
        return source;
    }

    public async Task<bool> DeleteSource(int id)
    {
        var count = await _db.UserConsentSources.Where(s => s.Id == id).ExecuteDeleteAsync();
        return count > 0;
    }
}
