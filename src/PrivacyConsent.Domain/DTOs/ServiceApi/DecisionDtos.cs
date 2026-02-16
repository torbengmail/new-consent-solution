using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class UserConsentDecisionRequest
{
    [Range(1, int.MaxValue)]
    public int ConsentExpressionId { get; set; }
    public int? ParentConsentExpressionId { get; set; }
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int IdTypeId { get; set; }
    public bool IsAgreed { get; set; }
    [Range(1, int.MaxValue)]
    public int UserConsentSourceId { get; set; }
    [Required]
    public string PresentedLanguage { get; set; } = string.Empty;
    public JsonElement? ChangeContext { get; set; }
    public DateTime? DecisionDate { get; set; }
}

public class UserConsentDecisionForUserApiRequest
{
    public int ConsentExpressionId { get; set; }
    public int? ParentConsentExpressionId { get; set; }
    public bool IsAgreed { get; set; }
    public int UserConsentSourceId { get; set; }
    public string PresentedLanguage { get; set; } = string.Empty;
    public JsonElement? ChangeContext { get; set; }
    public DateTime? DecisionDate { get; set; }
}

public class UserConsentDecisionEnrichedDto
{
    public int ConsentExpressionId { get; set; }
    public ConsentTextDto Text { get; set; } = new();
    public bool? IsAgreed { get; set; }
    public DateTime? LastAskedDate { get; set; }
    public DateTime? LastDecisionDate { get; set; }
    public DateTime? LastSeenDate { get; set; }
    public int? RequestAttempts { get; set; }
    public int? UserConsentSourceId { get; set; }
    public string? UserId { get; set; }
    public int? IdTypeId { get; set; }
    public int? DecisionId { get; set; }
}

public class UserConsentDecisionEnrichedListItemDto : UserConsentDecisionEnrichedDto
{
    public new bool IsAgreed { get; set; }
    public int ConsentId { get; set; }
    public JsonElement? ChangeContext { get; set; }
    public int ConsentRank { get; set; }
    public ConsentGroupDto? ConsentGroup { get; set; }
}

public class ConsentGroupDto
{
    public int Id { get; set; }
    public int ConsentGroupRank { get; set; }
    public ConsentTextDto Text { get; set; } = new();
}

public class UserConsentDecisionShortDto
{
    public int ConsentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int IdTypeId { get; set; }
    public bool? IsAgreed { get; set; }
}

public class UserConsentDecisionBatchDto
{
    public List<UserConsentDecisionBatchItemDto> Decisions { get; set; } = [];
    public int Offset { get; set; }
    public int Limit { get; set; }
}

public class UserConsentDecisionBatchItemDto
{
    public int ConsentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int IdTypeId { get; set; }
    public List<UserIdDto> Ids { get; set; } = [];
    public bool IsAgreed { get; set; }
    public int? DecisionId { get; set; }
    public int OwnerId { get; set; }
    public JsonElement? ChangeContext { get; set; }
    public DateTime? LastDecisionDate { get; set; }
    public string? PresentedLanguage { get; set; }
    public int? ConsentExpressionId { get; set; }
    public int? ParentConsentExpressionId { get; set; }
    public int? UserConsentSourceId { get; set; }
    public int? ConsentTypeId { get; set; }
}

public class UserIdDto
{
    public string UserId { get; set; } = string.Empty;
    public int IdTypeId { get; set; }
}

public class UserConsentDecisionBatchVoDto
{
    public int Id { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class IdVoDto
{
    public int Id { get; set; }
}

public class UserConsentDecisionUniqueDto
{
    public int ConsentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int IdTypeId { get; set; }
}

public class DecisionHistoryItemDto
{
    public int ConsentId { get; set; }
    public int ConsentExpressionId { get; set; }
    public int? ParentConsentExpressionId { get; set; }
    public string PresentedLanguage { get; set; } = string.Empty;
    public JsonElement? ChangeContext { get; set; }
    public bool IsAgreed { get; set; }
    public DateTime Date { get; set; }
    public int UserConsentSourceId { get; set; }
}

public class RetractLastUserConsentDecisionRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int UserConsentSourceId { get; set; }
    [Range(1, int.MaxValue)]
    public int IdTypeId { get; set; }
    [Range(1, int.MaxValue)]
    public int ConsentId { get; set; }
}

public class UpdateLastUserConsentDecisionRequest : RetractLastUserConsentDecisionRequest
{
    public bool Value { get; set; }
}
