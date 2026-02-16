using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Domain.Models;

namespace PrivacyConsent.Data.Queries;

public class RbacQueries : IRbacQueries
{
    private readonly PrivacyDbContext _db;

    public RbacQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, UserIdentity>> GetUsersRolesPermissions()
    {
        var rows = await (
            from u in _db.Users
            from ur in _db.UserRoles.Where(ur => ur.UserId == u.Id).DefaultIfEmpty()
            from r in _db.Roles.Where(r => ur != null && r.Id == ur.RoleId).DefaultIfEmpty()
            from rp in _db.RolePermissions.Where(rp => r != null && rp.RoleId == r.Id).DefaultIfEmpty()
            from p in _db.Permissions.Where(p => rp != null && p.Id == rp.PermissionId).DefaultIfEmpty()
            from uo in _db.UserOwners.Where(uo => uo.UserId == u.Id).DefaultIfEmpty()
            select new
            {
                u.Id,
                u.Username,
                u.Password,
                u.Name,
                u.Email,
                u.IsConnectId,
                RoleName = r != null ? r.Name : null,
                PermissionName = p != null ? p.Name : null,
                OwnerId = uo != null ? (int?)uo.OwnerId : null
            }
        ).ToListAsync();

        var result = new Dictionary<string, UserIdentity>();

        foreach (var row in rows)
        {
            if (!result.TryGetValue(row.Username, out var identity))
            {
                identity = new UserIdentity
                {
                    Id = row.Id,
                    Username = row.Username,
                    Password = row.Password,
                    Name = row.Name,
                    Email = row.Email,
                    IsConnectId = row.IsConnectId
                };
                result[row.Username] = identity;
            }

            if (row.RoleName != null)
                identity.Roles.Add(row.RoleName);
            if (row.PermissionName != null)
                identity.Permissions.Add(row.PermissionName);
            if (row.OwnerId.HasValue)
                identity.Owners.Add(row.OwnerId.Value);
        }

        return result;
    }
}
