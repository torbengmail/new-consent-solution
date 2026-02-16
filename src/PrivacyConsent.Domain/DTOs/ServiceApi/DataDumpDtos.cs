namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class DataDumpDecisionItemDto
{
    public int SourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public int ConsentId { get; set; }
    public int ExpressionId { get; set; }
    public string ExpressionTitle { get; set; } = string.Empty;
    public string ExpressionText { get; set; } = string.Empty;
    public string ExpressionLegal { get; set; } = string.Empty;
    public string PresentedLanguage { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsAgreed { get; set; }
    public object? ChangeContext { get; set; }
}

public class DataDumpRequestAttemptItemDto
{
    public int? SourceId { get; set; }
    public string? SourceName { get; set; }
    public int ConsentId { get; set; }
    public int ExpressionId { get; set; }
    public string ExpressionTitle { get; set; } = string.Empty;
    public string ExpressionText { get; set; } = string.Empty;
    public string ExpressionLegal { get; set; } = string.Empty;
    public string PresentedLanguage { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class DataDumpDto
{
    public List<DataDumpDecisionItemDto> Decisions { get; set; } = [];
    public List<DataDumpRequestAttemptItemDto> RequestAttempts { get; set; } = [];
}
