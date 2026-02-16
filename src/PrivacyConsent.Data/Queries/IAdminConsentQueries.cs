using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IAdminConsentQueries
{
    Task<List<Consent>> GetConsents(IEnumerable<int> ownerIds);
    Task<Consent?> GetConsentById(int consentId);
    Task<Consent> CreateConsent(Consent consent);
    Task<Consent?> UpdateConsent(int consentId, Consent updated);
    Task<bool> SoftDeleteConsent(int consentId);
    Task<List<ConsentExpression>> GetExpressionsByConsentId(int consentId);
    Task<ConsentExpression?> GetExpressionById(int expressionId);
    Task<ConsentExpression> CreateExpression(ConsentExpression expression);
    Task<ConsentExpression?> UpdateExpression(int expressionId, ConsentExpression updated);
    Task SetExpressionTags(int expressionId, List<int> tagIds);
    Task UpsertExpressionText(int expressionId, string language, string title, string shortText, string longText);
    Task<List<ConsentExpressionTag>> GetTags(IEnumerable<int> ownerIds);
    Task<ConsentExpressionTag?> GetTagById(int tagId);
    Task<ConsentExpressionTag> CreateTag(string name, int ownerId);
    Task<bool> DeleteTag(int tagId);
}
