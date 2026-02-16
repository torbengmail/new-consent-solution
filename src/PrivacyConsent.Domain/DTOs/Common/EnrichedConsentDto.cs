namespace PrivacyConsent.Domain.DTOs.Common;

public class EnrichedConsentDto
{
    public Guid Uuid { get; set; }
    public string? UserId { get; set; }
    public int? IdTypeId { get; set; }
    public List<EnrichedIdDto> Ids { get; set; } = [];
    public bool IsAgreed { get; set; }
    public int? ConsentExpressionId { get; set; }
    public int? ConsentId { get; set; }
    public int? ConsentTypeId { get; set; }
    public string? ChangeContext { get; set; }
    public int? OwnerId { get; set; }
    public int? ProductId { get; set; }
    public int? DecisionId { get; set; }
    public DateTime? LastDecisionDate { get; set; }
    public DateTime? DecisionAuditDate { get; set; }
    public int? UserConsentSourceId { get; set; }

    // Extended fields (per owner)
    public string? ConsentName { get; set; }
    public string? ConsentExpressionName { get; set; }
    public string? ConsentExpressionDescription { get; set; }
    public string? Title { get; set; }
    public string? ShortText { get; set; }
    public string? LongText { get; set; }
    public string? PresentedLanguage { get; set; }
}

public class EnrichedIdDto
{
    public string? UserId { get; set; }
    public int? IdTypeId { get; set; }
}
