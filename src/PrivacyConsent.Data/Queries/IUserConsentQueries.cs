namespace PrivacyConsent.Data.Queries;

public interface IUserConsentQueries
{
    Task<int> UpsertUserConsent(Guid masterId, int consentId, int? consentExpressionId, int? parentConsentExpressionId, bool isAgreed, int? userConsentSourceId, string? presentedLanguage, string? changeContext, int? idTypeId, int? ownerId, string userId = "");
    Task<long> CreateAuditTrail(int decisionId, int? consentExpressionId, int? parentConsentExpressionId, bool isAgreed, string? presentedLanguage, int? userConsentSourceId, string? changeContext, string? userId, int? idTypeId);
    Task<List<DecisionHistoryRow>> ReadDecisionHistory(string userId, int idTypeId, int consentId);
    Task<List<UserConsentDecisionShortRow>> GetUserConsentDecisionsShort(List<(int ConsentId, string UserId, int IdTypeId)> requests);
    Task<List<UserConsentDecisionBatchRow>> GetUserConsentDecisionsBatch(int? ownerId, int? consentId, int offset, int limit);
    Task UpdateLastSeenDate(List<int> decisionIds);
    Task<int> RetractLastDecision(string userId, int idTypeId, int consentId, int sourceId);
    Task<int> UpdateLastDecision(string userId, int idTypeId, int consentId, int sourceId, bool value);
}
