using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data.Queries;

public interface IUserManagementQueries
{
    Task<List<User>> GetUsers();
    Task<User?> GetUserById(int userId);
    Task<User> CreateUser(string username, string? name, string? email, List<int>? roleIds, List<int>? ownerIds);
    Task<User?> UpdateUser(int userId, string? name, string? email, List<int>? roleIds, List<int>? ownerIds);
    Task<bool> DeleteUser(int userId);
    Task<List<Role>> GetRoles();
}
