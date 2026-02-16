using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public class UserManagementQueries : IUserManagementQueries
{
    private readonly PrivacyDbContext _db;

    public UserManagementQueries(PrivacyDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetUsers()
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOwners)
            .ToListAsync();
    }

    public async Task<User?> GetUserById(int userId)
    {
        return await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserOwners)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> CreateUser(string username, string? name, string? email,
        List<int>? roleIds, List<int>? ownerIds)
    {
        var user = new User
        {
            Username = username,
            Name = name,
            Email = email,
            IsConnectId = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (roleIds != null)
        {
            foreach (var roleId in roleIds)
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }

        if (ownerIds != null)
        {
            foreach (var ownerId in ownerIds)
                _db.UserOwners.Add(new UserOwner { UserId = user.Id, OwnerId = ownerId });
        }

        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateUser(int userId, string? name, string? email,
        List<int>? roleIds, List<int>? ownerIds)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        if (name != null) user.Name = name;
        if (email != null) user.Email = email;

        if (roleIds != null)
        {
            var existingRoles = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _db.UserRoles.RemoveRange(existingRoles);
            foreach (var roleId in roleIds)
                _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }

        if (ownerIds != null)
        {
            var existingOwners = await _db.UserOwners.Where(uo => uo.UserId == userId).ToListAsync();
            _db.UserOwners.RemoveRange(existingOwners);
            foreach (var ownerId in ownerIds)
                _db.UserOwners.Add(new UserOwner { UserId = userId, OwnerId = ownerId });
        }

        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUser(int userId)
    {
        var count = await _db.Users.Where(u => u.Id == userId).ExecuteDeleteAsync();
        return count > 0;
    }

    public async Task<List<Role>> GetRoles()
    {
        return await _db.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }
}
