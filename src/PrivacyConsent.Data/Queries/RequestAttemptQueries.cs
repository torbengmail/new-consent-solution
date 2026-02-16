using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class RequestAttemptQueries : IRequestAttemptQueries
{
    private readonly PrivacyDbContext _db;

    public RequestAttemptQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task RegisterRequestAttempt(Guid masterId, int consentId, int consentExpressionId,
        string? presentedLanguage, int? userConsentSourceId, string userId = "", int idTypeId = 0)
    {
        var existing = await _db.RequestAttempts
            .FirstOrDefaultAsync(ra =>
                ra.MasterId == masterId &&
                ra.ConsentId == consentId &&
                ra.ConsentExpressionId == consentExpressionId);

        if (existing != null)
        {
            existing.AttemptsCount++;
            existing.LastAskedDate = DateTime.UtcNow;
            existing.PresentedLanguage = presentedLanguage;
            existing.UserConsentSourceId = userConsentSourceId;
        }
        else
        {
            _db.RequestAttempts.Add(new RequestAttempt
            {
                MasterId = masterId,
                ConsentId = consentId,
                ConsentExpressionId = consentExpressionId,
                PresentedLanguage = presentedLanguage,
                LastAskedDate = DateTime.UtcNow,
                AttemptsCount = 1,
                UserConsentSourceId = userConsentSourceId,
                UserId = userId,
                IdTypeId = idTypeId
            });
        }

        // Save first to get the attempt ID for new inserts
        await _db.SaveChangesAsync();

        // Create audit trail
        var attempt = existing ?? await _db.RequestAttempts
            .FirstAsync(ra =>
                ra.MasterId == masterId &&
                ra.ConsentId == consentId &&
                ra.ConsentExpressionId == consentExpressionId);

        _db.RequestAttemptAuditTrails.Add(new RequestAttemptAuditTrail
        {
            AttemptId = attempt.Id,
            Date = DateTime.UtcNow,
            PresentedLanguage = presentedLanguage,
            ConsentExpressionId = consentExpressionId,
            UserConsentSourceId = userConsentSourceId,
            UserId = userId,
            IdTypeId = idTypeId
        });

        await _db.SaveChangesAsync();
    }
}
