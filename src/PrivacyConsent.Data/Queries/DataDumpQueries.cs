using Microsoft.EntityFrameworkCore;

namespace PrivacyConsent.Data.Queries;

public class DataDumpQueries : IDataDumpQueries
{
    private readonly PrivacyDbContext _db;

    public DataDumpQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<DecisionDataRecord>> GetUserDecisionDataRecords(string userId, int idTypeId)
    {
        return await (
            from uc in _db.UserConsents
            join a in _db.UserConsentAuditTrails on uc.Id equals a.DecisionId
            join ce in _db.ConsentExpressions on a.ConsentExpressionId equals ce.Id
            join m in _db.MasterIds on uc.MasterId equals m.Id
            from cet in _db.ConsentExpressionTexts
                .Where(t => t.ConsentExpressionId == ce.Id && t.Language == (a.PresentedLanguage ?? "en"))
                .DefaultIfEmpty()
            from src in _db.UserConsentSources
                .Where(s => s.Id == a.UserConsentSourceId)
                .DefaultIfEmpty()
            where m.UserId == userId && m.IdTypeId == idTypeId
            orderby a.Date descending
            select new DecisionDataRecord
            {
                SourceId = src != null ? src.Id : 0,
                SourceName = src != null ? src.Name : "",
                ConsentId = uc.ConsentId,
                ExpressionId = ce.Id,
                ExpressionTitle = cet != null ? cet.Title : "",
                ExpressionText = cet != null ? cet.ShortText : "",
                ExpressionLegal = cet != null ? cet.LongText : "",
                PresentedLanguage = a.PresentedLanguage ?? "en",
                Date = a.Date,
                IsAgreed = a.IsAgreed,
                ChangeContext = a.ChangeContext
            }
        ).ToListAsync();
    }

    public async Task<List<RequestAttemptDataRecord>> GetUserRequestAttemptDataRecords(string userId, int idTypeId)
    {
        return await (
            from ra in _db.RequestAttempts
            join raa in _db.RequestAttemptAuditTrails on ra.Id equals raa.AttemptId
            join ce in _db.ConsentExpressions on raa.ConsentExpressionId equals ce.Id
            join m in _db.MasterIds on ra.MasterId equals m.Id
            from cet in _db.ConsentExpressionTexts
                .Where(t => t.ConsentExpressionId == ce.Id && t.Language == (raa.PresentedLanguage ?? "en"))
                .DefaultIfEmpty()
            from src in _db.UserConsentSources
                .Where(s => s.Id == raa.UserConsentSourceId)
                .DefaultIfEmpty()
            where m.UserId == userId && m.IdTypeId == idTypeId
            orderby raa.Date descending
            select new RequestAttemptDataRecord
            {
                SourceId = src != null ? (int?)src.Id : null,
                SourceName = src != null ? src.Name : null,
                ConsentId = ra.ConsentId,
                ExpressionId = ce.Id,
                ExpressionTitle = cet != null ? cet.Title : "",
                ExpressionText = cet != null ? cet.ShortText : "",
                ExpressionLegal = cet != null ? cet.LongText : "",
                PresentedLanguage = raa.PresentedLanguage ?? "en",
                Date = raa.Date
            }
        ).ToListAsync();
    }

}

public record DecisionDataRecord
{
    public int SourceId { get; init; }
    public string SourceName { get; init; } = "";
    public int ConsentId { get; init; }
    public int ExpressionId { get; init; }
    public string ExpressionTitle { get; init; } = "";
    public string ExpressionText { get; init; } = "";
    public string ExpressionLegal { get; init; } = "";
    public string PresentedLanguage { get; init; } = "";
    public DateTime Date { get; init; }
    public bool IsAgreed { get; init; }
    public string? ChangeContext { get; init; }
}

public record RequestAttemptDataRecord
{
    public int? SourceId { get; init; }
    public string? SourceName { get; init; }
    public int ConsentId { get; init; }
    public int ExpressionId { get; init; }
    public string ExpressionTitle { get; init; } = "";
    public string ExpressionText { get; init; } = "";
    public string ExpressionLegal { get; init; } = "";
    public string PresentedLanguage { get; init; } = "";
    public DateTime Date { get; init; }
}
