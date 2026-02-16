using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface ITranslationQueries
{
    Task<Dictionary<string, string>> GetLanguageTranslations(string langCode, int ownerId, int? productId);
    Task<List<TranslationWithOwnerRow>> GetLanguageTranslationsMultiOwners(string langCode, IEnumerable<int> ownerIds, string textType);
    Task<List<AdminTranslation>> GetTranslations(int ownerId, int? productId);
    Task UpsertAdminTranslations(int ownerId, int? productId, string langCode, Dictionary<string, Dictionary<string, Dictionary<string, string>>> texts);
}
