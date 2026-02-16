using System.ComponentModel.DataAnnotations;

namespace PrivacyConsent.Domain.DTOs.ServiceApi;

public class IdTypeRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class IdTypeResponse
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
}

public class IdMappingRequest
{
    [Range(1, int.MaxValue)]
    public int IdTypeId { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class IdMappingResponse
{
    public int IdTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
}
