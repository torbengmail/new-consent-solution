using Microsoft.EntityFrameworkCore;

namespace PrivacyConsent.Data.Queries;

public class EnricherQueries
{
    private readonly PrivacyDbContext _db;

    public EnricherQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Port of enricher repository.clj select-decision-data-query.
    /// Retrieves complete decision audit data with all related consent/expression metadata.
    /// </summary>
    public async Task<List<ConsentRelationRow>> GetConsentRelations(long decisionAuditId)
    {
        return await (
            from uca in _db.UserConsentAuditTrails
            join uc in _db.UserConsents on uca.DecisionId equals uc.Id
            join c in _db.Consents.IgnoreQueryFilters() on uc.ConsentId equals c.Id
            join ct in _db.ConsentTypes on c.ConsentTypeId equals ct.Id
            join m in _db.MasterIds on uc.MasterId equals m.Id
            join ce in _db.ConsentExpressions on uca.ConsentExpressionId equals ce.Id
            from cet in _db.ConsentExpressionTexts
                .Where(t => t.ConsentExpressionId == ce.Id && t.Language == uca.PresentedLanguage)
                .DefaultIfEmpty()
            where uca.Id == decisionAuditId
            select new ConsentRelationRow
            {
                ConsentExpressionId = ce.Id,
                ConsentExpressionName = ce.Name,
                ConsentExpressionDescription = ce.Description,
                ConsentId = c.Id,
                ConsentName = c.Name,
                ConsentTypeId = c.ConsentTypeId,
                OwnerId = c.OwnerId,
                ProductId = c.ProductId,
                DecisionId = uca.DecisionId,
                DecisionAuditDate = uca.Date,
                LastDecisionDate = uc.LastDecisionDate,
                UserId = uca.UserId,
                IdTypeId = uca.IdTypeId,
                ChangeContext = uca.ChangeContext,
                UserConsentSourceId = uca.UserConsentSourceId,
                IsAgreed = uca.IsAgreed,
                PresentedLanguage = cet != null ? cet.Language : null,
                Title = cet != null ? cet.Title : null,
                ShortText = cet != null ? cet.ShortText : null,
                LongText = cet != null ? cet.LongText : null,
                MasterUserId = m.UserId,
                MasterIdTypeId = m.IdTypeId
            }
        ).ToListAsync();
    }

    public record ConsentRelationRow
    {
        public int ConsentExpressionId { get; init; }
        public string? ConsentExpressionName { get; init; }
        public string? ConsentExpressionDescription { get; init; }
        public int ConsentId { get; init; }
        public string ConsentName { get; init; } = "";
        public int ConsentTypeId { get; init; }
        public int? OwnerId { get; init; }
        public int? ProductId { get; init; }
        public int DecisionId { get; init; }
        public DateTime DecisionAuditDate { get; init; }
        public DateTime? LastDecisionDate { get; init; }
        public string? UserId { get; init; }
        public int? IdTypeId { get; init; }
        public string? ChangeContext { get; init; }
        public int? UserConsentSourceId { get; init; }
        public bool IsAgreed { get; init; }
        public string? PresentedLanguage { get; init; }
        public string? Title { get; init; }
        public string? ShortText { get; init; }
        public string? LongText { get; init; }
        public string MasterUserId { get; init; } = "";
        public int MasterIdTypeId { get; init; }
    }
}
