using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IDictionaryQueries
{
    Task<List<ConsentType>> GetConsentTypes();
    Task<List<PurposeCategory>> GetPurposeCategories();
    Task<List<ConsentExpressionTag>> GetExpressionTags();
    Task<List<ConsentExpressionStatus>> GetExpressionStatuses();
    Task<List<Language>> GetLanguages();
    Task<List<IdType>> GetIdTypes();
    Task<List<Owner>> GetOwners();
    Task<List<Product>> GetProducts();
    Task<List<Owner>> GetOwnersWithProducts();
}
