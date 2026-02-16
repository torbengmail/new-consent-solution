namespace PrivacyConsent.Data.Queries;

public interface IRequestAttemptQueries
{
    Task RegisterRequestAttempt(Guid masterId, int consentId, int consentExpressionId, string? presentedLanguage, int? userConsentSourceId, string userId = "", int idTypeId = 0);
}
