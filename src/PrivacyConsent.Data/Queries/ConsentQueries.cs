using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class ConsentQueries : IConsentQueries
{
    private readonly PrivacyDbContext _db;

    public ConsentQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<int?> GetConsentIdByExpression(int expressionId)
    {
        return await _db.ConsentExpressions
            .Where(ce => ce.Id == expressionId)
            .Select(ce => (int?)ce.ConsentId)
            .FirstOrDefaultAsync();
    }

    public async Task<(int ConsentId, int? OwnerId)?> GetConsentInfoByExpression(int expressionId)
    {
        var result = await (
            from ce in _db.ConsentExpressions
            join c in _db.Consents on ce.ConsentId equals c.Id
            where ce.Id == expressionId
            select new { c.Id, c.OwnerId }
        ).FirstOrDefaultAsync();

        return result == null ? null : (result.Id, result.OwnerId);
    }

    public async Task<int?> GetConsentOwner(int consentId)
    {
        return await _db.Consents
            .Where(c => c.Id == consentId)
            .Select(c => c.OwnerId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<int?>> GetConsentsOwners(IEnumerable<int> consentIds)
    {
        return await _db.Consents
            .Where(c => consentIds.Contains(c.Id))
            .Select(c => c.OwnerId)
            .ToListAsync();
    }

    public async Task<int?> GetExpressionOwner(int expressionId)
    {
        return await (
            from ce in _db.ConsentExpressions
            join c in _db.Consents on ce.ConsentId equals c.Id
            where ce.Id == expressionId
            select c.OwnerId
        ).FirstOrDefaultAsync();
    }

    public async Task<List<ConsentDto>> GetConsents(int? ownerId = null)
    {
        var query = from c in _db.Consents
                    join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
                    select new { c, ct };

        if (ownerId.HasValue)
            query = query.Where(x => x.c.OwnerId == ownerId.Value);

        return await query.Select(x => new ConsentDto
        {
            ConsentId = x.c.Id,
            Name = x.c.Name,
            DefaultOptIn = x.ct.DefaultOptIn,
            ConsentType = x.c.ConsentTypeId,
            ConsentTypeName = x.ct.Name
        }).ToListAsync();
    }

    public async Task<List<ConsentDto>> GetConsentsByUseCase(int ownerId, int useCaseId)
    {
        return await (
            from c in _db.Consents
            join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
            join ucc in _db.UseCaseConsents on c.Id equals ucc.ConsentId
            where c.OwnerId == ownerId
                  && ucc.UseCaseId == useCaseId
                  && (c.ExpirationDate == DateTime.MaxValue || c.ExpirationDate > DateTime.UtcNow)
            select new ConsentDto
            {
                ConsentId = c.Id,
                Name = c.Name,
                DefaultOptIn = ct.DefaultOptIn,
                ConsentType = c.ConsentTypeId,
                ConsentTypeName = ct.Name
            }
        ).ToListAsync();
    }

    public async Task<ConsentDto?> GetConsentById(int consentId)
    {
        return await (
            from c in _db.Consents
            join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
            where c.Id == consentId
            select new ConsentDto
            {
                ConsentId = c.Id,
                Name = c.Name,
                DefaultOptIn = ct.DefaultOptIn,
                ConsentType = c.ConsentTypeId,
                ConsentTypeName = ct.Name
            }
        ).FirstOrDefaultAsync();
    }

    public async Task<Dictionary<int, (int ConsentId, int? OwnerId)>> GetConsentInfoByExpressions(
        IEnumerable<int> expressionIds)
    {
        var ids = expressionIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<int, (int, int?)>();

        return await (
            from ce in _db.ConsentExpressions
            join c in _db.Consents on ce.ConsentId equals c.Id
            where ids.Contains(ce.Id)
            select new { ce.Id, ConsentId = c.Id, c.OwnerId }
        ).ToDictionaryAsync(r => r.Id, r => (r.ConsentId, r.OwnerId));
    }

}

public record ConsentDto
{
    public int ConsentId { get; init; }
    public string Name { get; init; } = "";
    public bool DefaultOptIn { get; init; }
    public int ConsentType { get; init; }
    public string ConsentTypeName { get; init; } = "";
}
