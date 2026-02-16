using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrivacyConsent.Data.Entities;

[Table("consent", Schema = "consent")]
public class Consent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("purpose_id")]
    public int? PurposeId { get; set; }

    [Column("consent_type_id")]
    public int ConsentTypeId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("hide_by_default")]
    public bool HideByDefault { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    [Column("modified_date")]
    public DateTime ModifiedDate { get; set; }

    [Column("special_data_category_id")]
    public int? SpecialDataCategoryId { get; set; }

    [Column("data_source_id")]
    public int? DataSourceId { get; set; }

    [Column("processing_type_id")]
    public int? ProcessingTypeId { get; set; }

    [Column("parent_consent_id")]
    public int? ParentConsentId { get; set; }

    [Column("is_group")]
    public bool IsGroup { get; set; }

    [Column("expiration_date")]
    public DateTime ExpirationDate { get; set; }

    [Column("consent_rank")]
    public int ConsentRank { get; set; }

    [Column("delete_at")]
    public DateTime? DeleteAt { get; set; }

    public ConsentType? ConsentType { get; set; }
    public ICollection<ConsentExpression> Expressions { get; set; } = [];
    public ICollection<UserConsent> UserConsents { get; set; } = [];
}

[Table("consent_type", Schema = "consent")]
public class ConsentType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("default_opt_in")]
    public bool DefaultOptIn { get; set; }

    [Column("hide_by_default")]
    public bool? HideByDefault { get; set; }
}

[Table("consent_expression", Schema = "consent")]
public class ConsentExpression
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("consent_id")]
    public int ConsentId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    [Column("modified_date")]
    public DateTime ModifiedDate { get; set; }

    public Consent? Consent { get; set; }
    public ConsentExpressionStatus? Status { get; set; }
    public ICollection<ConsentExpressionText> Texts { get; set; } = [];
    public ICollection<ConsentExpressionTagList> TagList { get; set; } = [];
}

[Table("consent_expression_status", Schema = "consent")]
public class ConsentExpressionStatus
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("consent_expression_text", Schema = "consent")]
public class ConsentExpressionText
{
    [Column("consent_expression_id")]
    public int ConsentExpressionId { get; set; }

    [Column("language")]
    public string Language { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("short_text")]
    public string ShortText { get; set; } = string.Empty;

    [Column("long_text")]
    public string LongText { get; set; } = string.Empty;

    public ConsentExpression? ConsentExpression { get; set; }
}

[Table("consent_expression_tag", Schema = "consent")]
public class ConsentExpressionTag
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("owner_id")]
    public int? OwnerId { get; set; }
}

[Table("consent_expression_tag_list", Schema = "consent")]
public class ConsentExpressionTagList
{
    [Column("consent_expression_id")]
    public int ConsentExpressionId { get; set; }

    [Column("consent_expression_tag_id")]
    public int ConsentExpressionTagId { get; set; }

    public ConsentExpression? ConsentExpression { get; set; }
    public ConsentExpressionTag? Tag { get; set; }
}

[Table("language", Schema = "consent")]
public class Language
{
    [Key]
    [Column("name")]
    public string Id { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("flag_key")]
    public string? FlagKey { get; set; }
}

[Table("user_consent", Schema = "consent")]
public class UserConsent
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("master_id")]
    public Guid? MasterId { get; set; }

    [Column("consent_id")]
    public int ConsentId { get; set; }

    [Column("consent_expression_id")]
    public int? ConsentExpressionId { get; set; }

    [Column("parent_consent_expression_id")]
    public int? ParentConsentExpressionId { get; set; }

    [Column("is_agreed")]
    public bool IsAgreed { get; set; }

    [Column("last_decision_date")]
    public DateTime? LastDecisionDate { get; set; }

    [Column("last_seen_date")]
    public DateTime? LastSeenDate { get; set; }

    [Column("user_consent_source_id")]
    public int? UserConsentSourceId { get; set; }

    [Column("presented_language")]
    public string? PresentedLanguage { get; set; }

    [Column("change_context", TypeName = "jsonb")]
    public string? ChangeContext { get; set; }

    [Column("id_type_id")]
    public int? IdTypeId { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("ext_date")]
    public bool ExtDate { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = "";

    public Consent? Consent { get; set; }
    public MasterId? Master { get; set; }
    public ICollection<UserConsentAuditTrail> AuditTrails { get; set; } = [];
}

[Table("user_consent_audit_trail", Schema = "consent")]
public class UserConsentAuditTrail
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("decision_id")]
    public int DecisionId { get; set; }

    [Column("consent_expression_id")]
    public int? ConsentExpressionId { get; set; }

    [Column("parent_consent_expression_id")]
    public int? ParentConsentExpressionId { get; set; }

    [Column("is_agreed")]
    public bool IsAgreed { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("presented_language")]
    public string? PresentedLanguage { get; set; }

    [Column("user_consent_source_id")]
    public int? UserConsentSourceId { get; set; }

    [Column("change_context", TypeName = "jsonb")]
    public string? ChangeContext { get; set; }

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("id_type_id")]
    public int? IdTypeId { get; set; }

    public UserConsent? Decision { get; set; }
}

[Table("request_attempt", Schema = "consent")]
public class RequestAttempt
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("master_id")]
    public Guid? MasterId { get; set; }

    [Column("consent_id")]
    public int ConsentId { get; set; }

    [Column("consent_expression_id")]
    public int ConsentExpressionId { get; set; }

    [Column("presented_language")]
    public string? PresentedLanguage { get; set; }

    [Column("last_asked_date")]
    public DateTime? LastAskedDate { get; set; }

    [Column("attempts_count")]
    public int AttemptsCount { get; set; }

    [Column("user_consent_source_id")]
    public int? UserConsentSourceId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = "";

    [Column("id_type_id")]
    public int IdTypeId { get; set; }
}

[Table("request_attempt_audit_trail", Schema = "consent")]
public class RequestAttemptAuditTrail
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("attempt_id")]
    public int AttemptId { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("presented_language")]
    public string? PresentedLanguage { get; set; }

    [Column("consent_expression_id")]
    public int? ConsentExpressionId { get; set; }

    [Column("user_consent_source_id")]
    public int? UserConsentSourceId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = "";

    [Column("id_type_id")]
    public int IdTypeId { get; set; }
}

[Table("user_consent_source", Schema = "consent")]
public class UserConsentSource
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("user_consent_source_type_id")]
    public int UserConsentSourceTypeId { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }
}

[Table("user_consent_source_type", Schema = "consent")]
public class UserConsentSourceType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("master_id", Schema = "consent")]
public class MasterId
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("id_type_id")]
    public int IdTypeId { get; set; }

    [Column("is_device_id")]
    public bool IsDeviceId { get; set; }
}

[Table("id_type", Schema = "consent")]
public class IdType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("id_map", Schema = "consent")]
public class IdMap
{
    [Key]
    [Column("id_type_id")]
    public int IdTypeId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("admin_translation", Schema = "consent")]
public class AdminTranslation
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("lang_code")]
    public string LangCode { get; set; } = string.Empty;

    [Column("translations", TypeName = "jsonb")]
    public string? Translations { get; set; }

    [Column("augmented_translations", TypeName = "jsonb")]
    public string? AugmentedTranslations { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }
}

[Table("user_data_cache", Schema = "consent")]
public class UserDataCache
{
    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("id_type_id")]
    public int IdTypeId { get; set; }

    [Column("data_key")]
    public string DataKey { get; set; } = string.Empty;

    [Column("data_value", TypeName = "jsonb")]
    public string? DataValue { get; set; }

    [Column("modified_date")]
    public DateTime ModifiedDate { get; set; }
}

[Table("dsr_tracking", Schema = "consent")]
public class DsrTracking
{
    [Column("ticket_id")]
    public string TicketId { get; set; } = string.Empty;

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("id_type_id")]
    public int IdTypeId { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("created_date")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_date")]
    public DateTime? UpdatedAt { get; set; }
}

[Table("user_consent_decisions_batch", Schema = "consent")]
public class UserConsentDecisionsBatch
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [Column("download_url")]
    public string? DownloadUrl { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

[Table("skin", Schema = "consent")]
public class Skin
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("skin")]
    public string? SkinData { get; set; }

    [Column("hide_sections")]
    public string? HideSections { get; set; }

    [Column("owner_id")]
    public int? OwnerId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("referrer")]
    public int? Referrer { get; set; }
}

[Table("test_user_group", Schema = "consent")]
public class TestUserGroup
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("test_user", Schema = "consent")]
public class TestUser
{
    [Column("group_id")]
    public int GroupId { get; set; }

    [Column("user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("id_type_id")]
    public int IdTypeId { get; set; }
}

// RBAC entities

[Table("permission", Schema = "consent")]
public class Permission
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[Table("role", Schema = "consent")]
public class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

[Table("role_permission", Schema = "consent")]
public class RolePermission
{
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("permission_id")]
    public int PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

[Table("user", Schema = "consent")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("password")]
    public string? Password { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("is_connect_id")]
    public bool IsConnectId { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserOwner> UserOwners { get; set; } = [];
}

[Table("user_role", Schema = "consent")]
public class UserRole
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}

[Table("user_owner", Schema = "consent")]
public class UserOwner
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("owner_id")]
    public int OwnerId { get; set; }

    public User? User { get; set; }
}

[Table("admin_api_audit_trail", Schema = "consent")]
public class AdminApiAuditTrail
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("entity_type")]
    public string? EntityType { get; set; }

    [Column("entity_id")]
    public string? EntityId { get; set; }

    [Column("details", TypeName = "jsonb")]
    public string? Details { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }
}

[Table("use_case_consent", Schema = "consent")]
public class UseCaseConsent
{
    [Column("use_case_id")]
    public int UseCaseId { get; set; }

    [Column("consent_id")]
    public int ConsentId { get; set; }

    public Consent? Consent { get; set; }
}

[Table("product_connect_id", Schema = "consent")]
public class ProductConnectId
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("connect_id_service")]
    public string? ConnectIdService { get; set; }
}
