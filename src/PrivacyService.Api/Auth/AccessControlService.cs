using Microsoft.Extensions.Caching.Memory;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Models;

namespace PrivacyService.Api.Auth;

public class AccessControlService : IAccessControlService
{
    private readonly IRbacQueries _rbacQueries;
    private readonly IConsentQueries _consentQueries;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccessControlService> _logger;
    private readonly TimeSpan _ttl;

    private const string CacheKey = "users_roles_permissions";

    public AccessControlService(
        IRbacQueries rbacQueries,
        IConsentQueries consentQueries,
        IMemoryCache cache,
        IConfiguration config,
        ILogger<AccessControlService> logger)
    {
        _rbacQueries = rbacQueries;
        _consentQueries = consentQueries;
        _cache = cache;
        _logger = logger;
        _ttl = TimeSpan.FromMilliseconds(config.GetValue<int>("UsersRolesPermissionsTtl", 60000));
    }

    public async Task<UserIdentity?> AuthenticateBasicAsync(string username, string password)
    {
        var users = await GetUsersRolesPermissionsAsync();

        if (!users.TryGetValue(username, out var identity))
            return null;

        if (identity.IsConnectId)
            return null;

        if (string.IsNullOrEmpty(identity.Password))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, identity.Password))
            return null;

        return identity;
    }

    public async Task<Dictionary<string, UserIdentity>> GetUsersRolesPermissionsAsync()
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _ttl;
            return await _rbacQueries.GetUsersRolesPermissions();
        }) ?? new Dictionary<string, UserIdentity>();
    }

    public bool HasAccess(UserIdentity identity, string dataType, object? data)
    {
        return dataType switch
        {
            "owner-id" => data is int ownerId ? identity.Owners.Contains(ownerId) : true,
            "owner-ids" => data is IEnumerable<int> ownerIds ? ownerIds.All(id => identity.Owners.Contains(id)) : true,
            _ => true
        };
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
    }
}
