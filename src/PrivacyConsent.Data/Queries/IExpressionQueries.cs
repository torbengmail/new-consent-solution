namespace PrivacyConsent.Data.Queries;

public interface IExpressionQueries
{
    Task<List<ExpressionWithDecisionRow>> GetExpressionsByConsentId(int consentId, string userId, int idTypeId, string language, string? tag);
    Task<List<ExpressionListItemRow>> GetRandExpressionsByProductId(int ownerId, int? productId, string userId, int idTypeId, string language, string? tag);
}
