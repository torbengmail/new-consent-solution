using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class UserConsentQueries : IUserConsentQueries
{
    private readonly PrivacyDbContext _db;

    public UserConsentQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<int> UpsertUserConsent(Guid masterId, int consentId, int? consentExpressionId,
        int? parentConsentExpressionId, bool isAgreed, int? userConsentSourceId,
        string? presentedLanguage, string? changeContext, int? idTypeId, int? ownerId,
        string userId = "")
    {
        var existing = await _db.UserConsents
            .FirstOrDefaultAsync(uc => uc.MasterId == masterId && uc.ConsentId == consentId);

        if (existing != null)
        {
            existing.ConsentExpressionId = consentExpressionId;
            existing.ParentConsentExpressionId = parentConsentExpressionId;
            existing.IsAgreed = isAgreed;
            existing.LastDecisionDate = DateTime.UtcNow;
            existing.UserConsentSourceId = userConsentSourceId;
            existing.PresentedLanguage = presentedLanguage;
            existing.ChangeContext = changeContext;
            existing.IdTypeId = idTypeId;
            existing.OwnerId = ownerId;
            if (!string.IsNullOrEmpty(userId))
                existing.UserId = userId;
            await _db.SaveChangesAsync();
            return existing.Id;
        }

        var userConsent = new UserConsent
        {
            MasterId = masterId,
            ConsentId = consentId,
            ConsentExpressionId = consentExpressionId,
            ParentConsentExpressionId = parentConsentExpressionId,
            IsAgreed = isAgreed,
            LastDecisionDate = DateTime.UtcNow,
            UserConsentSourceId = userConsentSourceId,
            PresentedLanguage = presentedLanguage,
            ChangeContext = changeContext,
            IdTypeId = idTypeId,
            OwnerId = ownerId,
            UserId = userId
        };

        _db.UserConsents.Add(userConsent);
        await _db.SaveChangesAsync();
        return userConsent.Id;
    }

    public async Task<long> CreateAuditTrail(int decisionId, int? consentExpressionId,
        int? parentConsentExpressionId, bool isAgreed, string? presentedLanguage,
        int? userConsentSourceId, string? changeContext, string? userId, int? idTypeId)
    {
        var audit = new UserConsentAuditTrail
        {
            DecisionId = decisionId,
            ConsentExpressionId = consentExpressionId,
            ParentConsentExpressionId = parentConsentExpressionId,
            IsAgreed = isAgreed,
            Date = DateTime.UtcNow,
            PresentedLanguage = presentedLanguage,
            UserConsentSourceId = userConsentSourceId,
            ChangeContext = changeContext,
            UserId = userId,
            IdTypeId = idTypeId
        };

        _db.UserConsentAuditTrails.Add(audit);
        await _db.SaveChangesAsync();
        return audit.Id;
    }

    public async Task<List<DecisionHistoryRow>> ReadDecisionHistory(
        string userId, int idTypeId, int consentId)
    {
        return await (
            from a in _db.UserConsentAuditTrails
            join d in _db.UserConsents on a.DecisionId equals d.Id
            join m in _db.MasterIds on d.MasterId equals m.Id
            where m.UserId == userId && m.IdTypeId == idTypeId && d.ConsentId == consentId
            orderby a.Date descending
            select new DecisionHistoryRow
            {
                ConsentId = d.ConsentId,
                ConsentExpressionId = a.ConsentExpressionId ?? 0,
                ParentConsentExpressionId = a.ParentConsentExpressionId,
                PresentedLanguage = a.PresentedLanguage ?? "",
                ChangeContext = a.ChangeContext,
                IsAgreed = a.IsAgreed,
                Date = a.Date,
                UserConsentSourceId = a.UserConsentSourceId ?? 0
            }
        ).ToListAsync();
    }

    public async Task<List<UserConsentDecisionShortRow>> GetUserConsentDecisionsShort(
        List<(int ConsentId, string UserId, int IdTypeId)> requests)
    {
        var results = new List<UserConsentDecisionShortRow>();

        foreach (var req in requests)
        {
            var row = await (
                from c in _db.Consents
                join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
                from m in _db.MasterIds
                    .Where(m => m.UserId == req.UserId && m.IdTypeId == req.IdTypeId)
                from uc in _db.UserConsents
                    .Where(uc => uc.MasterId == m.Id && uc.ConsentId == c.Id)
                    .DefaultIfEmpty()
                where c.Id == req.ConsentId
                select new UserConsentDecisionShortRow
                {
                    ConsentId = c.Id,
                    UserId = m.UserId,
                    IdTypeId = m.IdTypeId,
                    IsAgreed = uc != null ? (bool?)uc.IsAgreed : (ct.DefaultOptIn ? true : null)
                }
            ).FirstOrDefaultAsync();

            if (row != null)
                results.Add(row);
        }

        return results;
    }

    public async Task<List<UserConsentDecisionBatchRow>> GetUserConsentDecisionsBatch(
        int? ownerId, int? consentId, int offset, int limit)
    {
        var query = from d in _db.UserConsents
                    join c in _db.Consents on d.ConsentId equals c.Id
                    join m in _db.MasterIds on d.MasterId equals m.Id
                    select new { d, c, m };

        if (ownerId.HasValue)
            query = query.Where(x => x.c.OwnerId == ownerId.Value);
        if (consentId.HasValue)
            query = query.Where(x => x.c.Id == consentId.Value);

        var rawRows = await query
            .OrderBy(x => x.d.Id)
            .Skip(offset)
            .Take(limit)
            .Select(x => new
            {
                x.d.Id,
                x.d.ConsentId,
                x.d.IsAgreed,
                x.d.LastDecisionDate,
                x.d.ConsentExpressionId,
                x.d.ParentConsentExpressionId,
                x.d.UserConsentSourceId,
                x.d.ChangeContext,
                x.d.PresentedLanguage,
                x.c.OwnerId,
                x.c.ConsentTypeId,
                MUserId = x.m.UserId,
                MIdTypeId = x.m.IdTypeId
            })
            .ToListAsync();

        // Group by decision_id to aggregate master IDs
        var grouped = rawRows.GroupBy(r => r.Id).Select(g =>
        {
            var first = g.First();
            return new UserConsentDecisionBatchRow
            {
                DecisionId = first.Id,
                ConsentId = first.ConsentId,
                UserId = first.MUserId,
                IdTypeId = first.MIdTypeId,
                Ids = g.Select(r => new MasterIdRow { UserId = r.MUserId, IdTypeId = r.MIdTypeId }).ToList(),
                IsAgreed = first.IsAgreed,
                OwnerId = first.OwnerId ?? 0,
                ChangeContext = first.ChangeContext,
                LastDecisionDate = first.LastDecisionDate,
                PresentedLanguage = first.PresentedLanguage,
                ConsentExpressionId = first.ConsentExpressionId,
                ParentConsentExpressionId = first.ParentConsentExpressionId,
                UserConsentSourceId = first.UserConsentSourceId,
                ConsentTypeId = first.ConsentTypeId
            };
        }).ToList();

        return grouped;
    }

    public async Task UpdateLastSeenDate(List<int> decisionIds)
    {
        await _db.UserConsents
            .Where(uc => decisionIds.Contains(uc.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(uc => uc.LastSeenDate, DateTime.UtcNow));
    }

    public async Task<int> RetractLastDecision(string userId, int idTypeId, int consentId, int sourceId)
    {
        var master = await _db.MasterIds
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IdTypeId == idTypeId);
        if (master == null) return 0;

        var decision = await _db.UserConsents
            .FirstOrDefaultAsync(uc => uc.MasterId == master.Id && uc.ConsentId == consentId);
        if (decision == null) return 0;

        _db.UserConsents.Remove(decision);
        return await _db.SaveChangesAsync();
    }

    public async Task<int> UpdateLastDecision(string userId, int idTypeId, int consentId, int sourceId, bool value)
    {
        var master = await _db.MasterIds
            .FirstOrDefaultAsync(m => m.UserId == userId && m.IdTypeId == idTypeId);
        if (master == null) return 0;

        return await _db.UserConsents
            .Where(uc => uc.MasterId == master.Id && uc.ConsentId == consentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(uc => uc.IsAgreed, value)
                .SetProperty(uc => uc.LastDecisionDate, DateTime.UtcNow));
    }

}

public record DecisionHistoryRow
{
    public int ConsentId { get; init; }
    public int ConsentExpressionId { get; init; }
    public int? ParentConsentExpressionId { get; init; }
    public string PresentedLanguage { get; init; } = "";
    public string? ChangeContext { get; init; }
    public bool IsAgreed { get; init; }
    public DateTime Date { get; init; }
    public int UserConsentSourceId { get; init; }
}

public record UserConsentDecisionShortRow
{
    public int ConsentId { get; init; }
    public string UserId { get; init; } = "";
    public int IdTypeId { get; init; }
    public bool? IsAgreed { get; init; }
}

public record UserConsentDecisionBatchRow
{
    public int DecisionId { get; init; }
    public int ConsentId { get; init; }
    public string UserId { get; init; } = "";
    public int IdTypeId { get; init; }
    public List<MasterIdRow> Ids { get; init; } = [];
    public bool IsAgreed { get; init; }
    public int OwnerId { get; init; }
    public string? ChangeContext { get; init; }
    public DateTime? LastDecisionDate { get; init; }
    public string? PresentedLanguage { get; init; }
    public int? ConsentExpressionId { get; init; }
    public int? ParentConsentExpressionId { get; init; }
    public int? UserConsentSourceId { get; init; }
    public int? ConsentTypeId { get; init; }
}

public record MasterIdRow
{
    public string UserId { get; init; } = "";
    public int IdTypeId { get; init; }
}
