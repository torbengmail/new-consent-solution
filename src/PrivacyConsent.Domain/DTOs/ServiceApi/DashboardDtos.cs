using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class TreeConsentExpressionDto
{
    public int Id { get; set; }
    public bool Agreed { get; set; }
    public ConsentTextDto Text { get; set; } = new();
}

public class TreeConsentGroupDto
{
    public int Id { get; set; }
    public bool IsAgreed { get; set; }
    public ConsentTextDto Text { get; set; } = new();
    public List<TreeConsentExpressionDto> Consents { get; set; } = [];
}

public class TreeProductDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TreeConsentGroupDto> Groups { get; set; } = [];
    public List<TreeConsentExpressionDto> Consents { get; set; } = [];
}

public class DashboardRequest
{
    public string UserId { get; set; } = string.Empty;
    public int IdTypeId { get; set; }
    public int OwnerId { get; set; }
    public string Language { get; set; } = "en";
    public string Tag { get; set; } = "privacy-dashboard";
    public bool AdjustByContext { get; set; }
    public string? UserAccessToken { get; set; }
    public List<string>? UserProducts { get; set; }
}
