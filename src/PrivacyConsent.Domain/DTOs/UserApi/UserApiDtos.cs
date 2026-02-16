using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;

namespace PrivacyConsent.Domain.DTOs.UserApi;

public class DecisionAttributesDto
{
    public int ExpressionId { get; set; }
    public bool Agreed { get; set; }
    public ConsentTextDto Text { get; set; } = new();
}

public class DecisionGroupDto
{
    public int ExpressionId { get; set; }
    public bool Agreed { get; set; }
    public ConsentTextDto Text { get; set; } = new();
    public List<DecisionAttributesDto>? SubDecisions { get; set; }
}

public class DecisionsControllerDto
{
    public int? ControllerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<DecisionGroupDto> DecisionGroups { get; set; } = [];
}

public class DsrRequestCreatedResponse
{
    public string Message { get; set; } = string.Empty;
    public string? TicketId { get; set; }
}
