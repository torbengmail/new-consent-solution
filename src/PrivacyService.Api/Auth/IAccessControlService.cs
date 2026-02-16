using PrivacyConsent.Domain.Models;

namespace PrivacyService.Api.Auth;

public interface IAccessControlService
{
    Task<UserIdentity?> AuthenticateBasicAsync(string username, string password);
    Task<Dictionary<string, UserIdentity>> GetUsersRolesPermissionsAsync();
    bool HasAccess(UserIdentity identity, string dataType, object? data);
    void InvalidateCache();
}
