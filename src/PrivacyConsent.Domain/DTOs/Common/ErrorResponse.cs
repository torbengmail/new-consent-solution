namespace PrivacyConsent.Domain.DTOs.Common;

public class ErrorResponse
{
    public Guid? ErrorId { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
