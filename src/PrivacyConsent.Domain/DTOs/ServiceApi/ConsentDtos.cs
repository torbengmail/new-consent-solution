using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class ConsentDto
{
    public int ConsentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool DefaultOptIn { get; set; }
    public int ConsentType { get; set; }
    public string ConsentTypeName { get; set; } = string.Empty;
}

public class ConsentsResponse
{
    public List<ConsentDto> Consents { get; set; } = [];
}

public class ConsentExpressionTextDto
{
    public int ConsentExpressionId { get; set; }
    public ConsentTextDto Text { get; set; } = new();
}

public class LanguageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? FlagKey { get; set; }
}
