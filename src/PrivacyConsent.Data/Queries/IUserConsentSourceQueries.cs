using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IUserConsentSourceQueries
{
    Task<List<UserConsentSource>> GetSources(int? ownerId = null);
    Task<UserConsentSource?> GetSourceById(int id);
    Task<UserConsentSource> CreateSource(string name, string description, int sourceTypeId, int ownerId, int? productId);
    Task<UserConsentSource?> UpdateSource(int id, string? name, string? description, int? sourceTypeId, int ownerId, int? productId);
    Task<bool> DeleteSource(int id);
}
