namespace PrivacyConsent.Domain.Models;

public class UserIdentity
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool IsConnectId { get; set; }
    public HashSet<string> Roles { get; set; } = [];
    public HashSet<string> Permissions { get; set; } = [];
    public HashSet<int> Owners { get; set; } = [];
}
