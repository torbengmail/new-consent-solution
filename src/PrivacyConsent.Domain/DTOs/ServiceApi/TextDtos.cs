namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class TranslationAndSkinDto
{
    public Dictionary<string, string> Texts { get; set; } = new();
    public SkinThemeDto Theme { get; set; } = new();
}

public class SkinThemeDto
{
    public int? Id { get; set; }
    public string? Skin { get; set; }
    public List<string?> HideSections { get; set; } = [];
    public int? OwnerId { get; set; }
    public int? ProductId { get; set; }
    public int? Referrer { get; set; }
}

public class MultiOwnerTextDto
{
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Dictionary<string, string>? Text { get; set; }
}
