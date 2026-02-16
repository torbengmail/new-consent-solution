using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class ExpressionQueries : IExpressionQueries
{
    private readonly PrivacyDbContext _db;

    public ExpressionQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<ExpressionWithDecisionRow>> GetExpressionsByConsentId(
        int consentId, string userId, int idTypeId, string language, string? tag)
    {
        // Published status = 2
        var query = from c in _db.Consents
                    join lang in _db.Languages on 1 equals 1
                    join ce in _db.ConsentExpressions on c.Id equals ce.ConsentId
                    join ct in _db.ConsentExpressionTexts
                        on new { Id = ce.Id, Lang = language } equals new { Id = ct.ConsentExpressionId, Lang = ct.Language }
                        into textJoin
                    from ct in textJoin.DefaultIfEmpty()
                    join ctEn in _db.ConsentExpressionTexts
                        on new { Id = ce.Id, Lang = "en" } equals new { Id = ctEn.ConsentExpressionId, Lang = ctEn.Language }
                        into textEnJoin
                    from ctEn in textEnJoin.DefaultIfEmpty()
                    join ces in _db.ConsentExpressionStatuses on ce.StatusId equals ces.Id
                    from m in _db.MasterIds
                        .Where(m => m.UserId == userId && m.IdTypeId == idTypeId)
                        .DefaultIfEmpty()
                    from uc in _db.UserConsents
                        .Where(uc => m != null && uc.MasterId == m.Id && uc.ConsentId == c.Id)
                        .DefaultIfEmpty()
                    from r in _db.RequestAttempts
                        .Where(r => m != null && r.MasterId == m.Id && r.ConsentExpressionId == ce.Id)
                        .DefaultIfEmpty()
                    where c.Id == consentId && ce.StatusId == 2
                    select new ExpressionWithDecisionRow
                    {
                        ConsentExpressionId = ce.Id,
                        Title = ct != null ? ct.Title : (ctEn != null ? ctEn.Title : ""),
                        ShortText = ct != null ? ct.ShortText : (ctEn != null ? ctEn.ShortText : ""),
                        LongText = ct != null ? ct.LongText : (ctEn != null ? ctEn.LongText : ""),
                        IsAgreed = uc != null ? (bool?)uc.IsAgreed : null,
                        LastAskedDate = r != null ? r.LastAskedDate : null,
                        LastDecisionDate = uc != null ? uc.LastDecisionDate : null,
                        LastSeenDate = uc != null ? uc.LastSeenDate : null,
                        RequestAttempts = r != null ? (int?)r.AttemptsCount : null,
                        UserConsentSourceId = uc != null ? uc.UserConsentSourceId : null,
                        UserId = m != null ? m.UserId : null,
                        IdTypeId = m != null ? (int?)m.IdTypeId : null,
                        DecisionId = uc != null ? (int?)uc.Id : null
                    };

        return await query.ToListAsync();
    }

    public async Task<List<ExpressionListItemRow>> GetRandExpressionsByProductId(
        int ownerId, int? productId, string userId, int idTypeId,
        string language, string? tag)
    {
        var query = from c in _db.Consents
                    join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
                    join ce in _db.ConsentExpressions on c.Id equals ce.ConsentId
                    join ces in _db.ConsentExpressionStatuses on ce.StatusId equals ces.Id
                    from cet in _db.ConsentExpressionTexts
                        .Where(t => t.ConsentExpressionId == ce.Id && t.Language == language)
                        .DefaultIfEmpty()
                    from cetEn in _db.ConsentExpressionTexts
                        .Where(t => t.ConsentExpressionId == ce.Id && t.Language == "en")
                        .DefaultIfEmpty()
                    from m in _db.MasterIds
                        .Where(m => m.UserId == userId && m.IdTypeId == idTypeId)
                        .DefaultIfEmpty()
                    from uc in _db.UserConsents
                        .Where(uc => m != null && uc.MasterId == m.Id && uc.ConsentId == c.Id)
                        .DefaultIfEmpty()
                    from r in _db.RequestAttempts
                        .Where(r => m != null && r.MasterId == m.Id && r.ConsentExpressionId == ce.Id)
                        .DefaultIfEmpty()
                    where c.OwnerId == ownerId
                          && ce.StatusId == 2
                          && (c.ExpirationDate == DateTime.MaxValue || c.ExpirationDate > DateTime.UtcNow)
                    select new ExpressionListItemRow
                    {
                        ConsentExpressionId = ce.Id,
                        ConsentId = c.Id,
                        Title = cet != null ? cet.Title : (cetEn != null ? cetEn.Title : ""),
                        ShortText = cet != null ? cet.ShortText : (cetEn != null ? cetEn.ShortText : ""),
                        LongText = cet != null ? cet.LongText : (cetEn != null ? cetEn.LongText : ""),
                        IsAgreed = uc != null ? uc.IsAgreed : ct.DefaultOptIn,
                        LastAskedDate = r != null ? r.LastAskedDate : null,
                        LastDecisionDate = uc != null ? uc.LastDecisionDate : null,
                        LastSeenDate = uc != null ? uc.LastSeenDate : null,
                        RequestAttempts = r != null ? (int?)r.AttemptsCount : null,
                        UserConsentSourceId = uc != null ? uc.UserConsentSourceId : null,
                        UserId = m != null ? m.UserId : null,
                        IdTypeId = m != null ? (int?)m.IdTypeId : null,
                        DecisionId = uc != null ? (int?)uc.Id : null,
                        ConsentRank = c.ConsentRank,
                        ChangeContext = uc != null ? uc.ChangeContext : null,
                        ParentConsentId = c.ParentConsentId,
                        IsGroup = c.IsGroup,
                        ParentConsentExpressionId = uc != null ? uc.ParentConsentExpressionId : null,
                        ConsentTypeHideByDefault = ct.HideByDefault ?? false,
                        ProductId = c.ProductId
                    };

        if (productId.HasValue)
            query = query.Where(x => x.ProductId == productId.Value);

        // Apply tag filter if specified
        if (!string.IsNullOrEmpty(tag))
        {
            var taggedExpressionIds = _db.ConsentExpressionTagLists
                .Where(tl => _db.ConsentExpressionTags
                    .Where(t => t.Name == tag)
                    .Select(t => t.Id)
                    .Contains(tl.ConsentExpressionTagId))
                .Select(tl => tl.ConsentExpressionId);

            query = query.Where(x => taggedExpressionIds.Contains(x.ConsentExpressionId));
        }

        return await query.OrderBy(x => x.ConsentRank).ToListAsync();
    }

}

public record ExpressionWithDecisionRow
{
    public int ConsentExpressionId { get; init; }
    public string Title { get; init; } = "";
    public string ShortText { get; init; } = "";
    public string LongText { get; init; } = "";
    public bool? IsAgreed { get; init; }
    public DateTime? LastAskedDate { get; init; }
    public DateTime? LastDecisionDate { get; init; }
    public DateTime? LastSeenDate { get; init; }
    public int? RequestAttempts { get; init; }
    public int? UserConsentSourceId { get; init; }
    public string? UserId { get; init; }
    public int? IdTypeId { get; init; }
    public int? DecisionId { get; init; }
}

public record ExpressionListItemRow : ExpressionWithDecisionRow
{
    public new bool IsAgreed { get; init; }
    public int ConsentId { get; init; }
    public int ConsentRank { get; init; }
    public string? ChangeContext { get; init; }
    public int? ParentConsentId { get; init; }
    public bool IsGroup { get; init; }
    public int? ParentConsentExpressionId { get; init; }
    public bool ConsentTypeHideByDefault { get; init; }
    public int? ProductId { get; init; }
}
