namespace PrivacyConsent.Domain.Constants;

public static class CacheConstants
{
    public const int DefaultRetentionHours = 24;
    public const int OneHourRetention = 1;
    public const int WeekRetentionHours = 7 * 24;
    public const int EmptyValue = -1;
    public const string PersonalDataCacheKey = "personal-data";

    public static string CreateCompositeKey(int ownerId, string key) => $"BU_{ownerId}:{key}";

    public static (int OwnerId, string Key) ParseCompositeKey(string compositeKey)
    {
        var match = System.Text.RegularExpressions.Regex.Match(compositeKey, @"BU_([0-9]+):(.*)");
        if (!match.Success)
            throw new ArgumentException($"Wrong cache key: {compositeKey}");

        return (int.Parse(match.Groups[1].Value), match.Groups[2].Value);
    }
}
