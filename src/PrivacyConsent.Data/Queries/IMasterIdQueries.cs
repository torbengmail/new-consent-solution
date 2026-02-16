using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IMasterIdQueries
{
    Task<MasterId?> GetOrCreateMasterId(string userId, int idTypeId);
    Task<MasterId?> GetMasterId(string userId, int idTypeId);
    Task<IdType> CreateIdType(string name);
    Task<IdMap> CreateIdMapping(int idTypeId, string name);
    Task<int> DeleteTestUser(string userId, int idTypeId);
}
