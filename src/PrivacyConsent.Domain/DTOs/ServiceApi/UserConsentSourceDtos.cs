using System.ComponentModel.DataAnnotations;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class UserConsentSourceRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int UserConsentSourceTypeId { get; set; }
    [Range(1, int.MaxValue)]
    public int OwnerId { get; set; }
    public int? ProductId { get; set; }
}

public class UserConsentSourceDto : UserConsentSourceRequest
{
    public int Id { get; set; }
}

public class UserConsentSourcePatchRequest
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? UserConsentSourceTypeId { get; set; }
    public int OwnerId { get; set; }
    public int? ProductId { get; set; }
}
