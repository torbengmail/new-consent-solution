using System.ComponentModel.DataAnnotations;
using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Domain.DTOs.AdminApi;

public class AdminApiConsentResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public int? PurposeId { get; set; }
    public int ConsentTypeId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int? SpecialDataCategoryId { get; set; }
    public int? DataSourceId { get; set; }
    public int? ProcessingTypeId { get; set; }
    public int? ProductId { get; set; }
    public bool HideByDefault { get; set; }
    public int? ParentConsentId { get; set; }
    public bool IsGroup { get; set; }
    public DateTime ExpirationDate { get; set; }
    public int ConsentRank { get; set; }
    public DateTime? DeleteAt { get; set; }
}

public class AdminApiConsentsListItemDto : AdminApiConsentResponse
{
    public List<AdminApiConsentResponse>? Channels { get; set; }
}

public class AdminApiConsentRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Description { get; set; }
    [Range(1, int.MaxValue)]
    public int OwnerId { get; set; }
    public int? PurposeId { get; set; }
    public int ConsentTypeId { get; set; }
    public int? SpecialDataCategoryId { get; set; }
    public int? DataSourceId { get; set; }
    public int? ProcessingTypeId { get; set; }
    public int? ProductId { get; set; }
    public bool HideByDefault { get; set; }
    public int? ParentConsentId { get; set; }
    public bool IsGroup { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class AdminApiExpressionText
{
    public string Language { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ShortText { get; set; } = string.Empty;
    public string LongText { get; set; } = string.Empty;
}

public class AdminApiExpressionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ConsentId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int StatusId { get; set; }
    public bool IsDefault { get; set; }
    public List<ReferenceDataItem> Tags { get; set; } = [];
    public List<AdminApiExpressionText> Texts { get; set; } = [];
}

public class AdminApiExpressionRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Description { get; set; }
    public int ConsentId { get; set; }
    public int StatusId { get; set; }
    public bool IsDefault { get; set; }
    public List<int>? TagIds { get; set; }
    public List<AdminApiExpressionText>? Texts { get; set; }
}

public class AdminApiReferenceDataDto
{
    public List<ReferenceDataItem> ConsentTypes { get; set; } = [];
    public List<ReferenceDataItem> ConsentPurposes { get; set; } = [];
    public List<OwnedReferenceDataItem> ExpressionTags { get; set; } = [];
    public List<ReferenceDataItem> ExpressionStatuses { get; set; } = [];
}

public class AdminApiUserInfoDto
{
    public List<LanguageInfoDto> Languages { get; set; } = [];
    public HashSet<string> Roles { get; set; } = [];
    public HashSet<string> Permissions { get; set; } = [];
    public List<OwnerWithProductsDto> Owners { get; set; } = [];
}

public class LanguageInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? FlagKey { get; set; }
}

public class OwnerWithProductsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ReferenceDataItem> Products { get; set; } = [];
}

public class AdminApiRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ReferenceDataItem> Permissions { get; set; } = [];
}

public class AdminApiUsersReferenceDataDto
{
    public List<AdminApiRoleDto> Roles { get; set; } = [];
    public List<ReferenceDataItem> Owners { get; set; } = [];
}

public class AdminApiUserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<ReferenceDataItem> Roles { get; set; } = [];
    public List<ReferenceDataItem> Owners { get; set; } = [];
}

public class AdminApiUserRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    [StringLength(255)]
    public string? Name { get; set; }
    public string? Email { get; set; }
    public List<int>? Roles { get; set; }
    public List<int>? Owners { get; set; }
}

public class AdminApiTagRequest
{
    public int? Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(1, int.MaxValue)]
    public int OwnerId { get; set; }
}

public class AdminApiTagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
}

public class AdminApiTextFieldRequest
{
    [Range(1, int.MaxValue)]
    public int OwnerId { get; set; }
    public int? ProductId { get; set; }
    [Required]
    [StringLength(10)]
    public string Language { get; set; } = string.Empty;
    [Required]
    [StringLength(100)]
    public string Page { get; set; } = string.Empty;
    [Required]
    [StringLength(255)]
    public string Key { get; set; } = string.Empty;
    [Required]
    public string Value { get; set; } = string.Empty;
}

public class AdminApiTextFieldResponse
{
    public string Value { get; set; } = string.Empty;
}

public class AdminTextsRequest
{
    public int OwnerId { get; set; }
    public int? ProductId { get; set; }
    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Texts { get; set; } = new();
}
