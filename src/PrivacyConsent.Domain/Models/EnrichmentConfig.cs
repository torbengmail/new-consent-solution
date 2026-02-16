namespace PrivacyConsent.Domain.Models;

public class EnrichmentConfig
{
    public HashSet<string> DefaultKeySet { get; set; } = [];
    public Dictionary<int, HashSet<string>> OwnerKeySetExtensions { get; set; } = new();
}
