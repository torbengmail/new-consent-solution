using System.Security.Claims;

namespace PrivacyService.Api.Middleware;

public static class OwnerAccessExtensions
{
    public static HashSet<int> GetOwnerIds(this ClaimsPrincipal user)
    {
        return user.FindAll("owner")
            .Select(c => int.TryParse(c.Value, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToHashSet();
    }

    public static HashSet<string> GetPermissions(this ClaimsPrincipal user)
    {
        return user.FindAll("permission")
            .Select(c => c.Value)
            .ToHashSet();
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        return user.GetPermissions().Contains(permission);
    }

    public static bool HasOwnerAccess(this ClaimsPrincipal user, int? ownerId)
    {
        if (ownerId == null) return true;
        return user.GetOwnerIds().Contains(ownerId.Value);
    }
}
