using PrivacyConsent.Domain.Models;

namespace PrivacyConsent.Data.Queries;

public interface IRbacQueries
{
    Task<Dictionary<string, UserIdentity>> GetUsersRolesPermissions();
}
