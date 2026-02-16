using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IDsrQueries
{
    Task<List<DsrTracking>> GetDsrRequests(string userId, int idTypeId, string type);
    Task<DsrTracking?> GetDsrRequest(string ticketId, string userId, int idTypeId, string type);
    Task CreateDsrRequest(string ticketId, string userId, int idTypeId, string type, string status);
    Task UpdateDsrRequest(string ticketId, string userId, int idTypeId, string type, string status);
}
