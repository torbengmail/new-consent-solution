using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class DataSubjectRightsRequestDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Right { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? OwnerId { get; set; }
}

public class DsrRightsDto
{
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Dictionary<string, bool> ReqStates { get; set; } = new();
}

public class PersonalDataFileProductDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class PersonalDataFileDto
{
    public string TicketId { get; set; } = string.Empty;
    public PersonalDataFileProductDto Product { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string? LastModified { get; set; }
}

public class PersonalDataByOwnerDto : ErrorResponse
{
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public List<PersonalDataFileDto> Links { get; set; } = [];
}

public class PersonalDataUploadLinkDto
{
    public string S3Link { get; set; } = string.Empty;
}

public class DeleteTestUserResponse
{
    public int RequestAttempt { get; set; }
    public int UserConsent { get; set; }
}
