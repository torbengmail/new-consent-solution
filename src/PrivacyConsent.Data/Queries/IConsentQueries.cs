namespace PrivacyConsent.Data.Queries;

public interface IConsentQueries
{
    Task<int?> GetConsentIdByExpression(int expressionId);
    Task<(int ConsentId, int? OwnerId)?> GetConsentInfoByExpression(int expressionId);
    Task<int?> GetConsentOwner(int consentId);
    Task<List<int?>> GetConsentsOwners(IEnumerable<int> consentIds);
    Task<int?> GetExpressionOwner(int expressionId);
    Task<List<ConsentDto>> GetConsents(int? ownerId = null);
    Task<List<ConsentDto>> GetConsentsByUseCase(int ownerId, int useCaseId);
    Task<ConsentDto?> GetConsentById(int consentId);
    Task<Dictionary<int, (int ConsentId, int? OwnerId)>> GetConsentInfoByExpressions(IEnumerable<int> expressionIds);
}
