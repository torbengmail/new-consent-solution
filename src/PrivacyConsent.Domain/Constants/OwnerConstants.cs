namespace PrivacyConsent.Domain.Constants;

public static class OwnerConstants
{
    public const int TdOwnerId = 1;
    public const int DenmarkOwnerId = 6;
    public const int CbbOwnerId = 7;

    public const string TdOwnerName = "Telenor Digital";
    public const string DenmarkOwnerName = "Telenor Denmark";
    public const string CbbOwnerName = "CBB";

    public const int DenmarkProductId = 2147483605;

    public const string TdOwnerTag = "td";

    public static readonly Dictionary<int, OwnerInfo> Owners = new()
    {
        [TdOwnerId] = new OwnerInfo(TdOwnerName, TdOwnerTag),
        [DenmarkOwnerId] = new OwnerInfo(DenmarkOwnerName, null),
        [CbbOwnerId] = new OwnerInfo(CbbOwnerName, null),
    };

    public static string? GetOwnerName(int ownerId) =>
        Owners.TryGetValue(ownerId, out var info) ? info.Name : null;
}

public record OwnerInfo(string Name, string? Tag);
