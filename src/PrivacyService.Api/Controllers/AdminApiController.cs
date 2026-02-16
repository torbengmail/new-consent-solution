using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data;
using PrivacyConsent.Data.Entities;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.AdminApi;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyService.Api.Auth;
using PrivacyService.Api.Filters;
using PrivacyService.Api.Middleware;

namespace PrivacyService.Api.Controllers;

[ApiController]
[Route("v1/adminapi")]
[Authorize]
public class AdminApiController : ControllerBase
{
    private readonly IAdminConsentQueries _consentQueries;
    private readonly IDictionaryQueries _dictionaryQueries;
    private readonly ITranslationQueries _translationQueries;
    private readonly IUserManagementQueries _userQueries;
    private readonly IAccessControlService _accessControl;
    private readonly PrivacyDbContext _db;
    private readonly ILogger<AdminApiController> _logger;

    public AdminApiController(
        IAdminConsentQueries consentQueries,
        IDictionaryQueries dictionaryQueries,
        ITranslationQueries translationQueries,
        IUserManagementQueries userQueries,
        IAccessControlService accessControl,
        PrivacyDbContext db,
        ILogger<AdminApiController> logger)
    {
        _consentQueries = consentQueries;
        _dictionaryQueries = dictionaryQueries;
        _translationQueries = translationQueries;
        _userQueries = userQueries;
        _accessControl = accessControl;
        _db = db;
        _logger = logger;
    }

    private HashSet<int> GetOwnerIds() => User.GetOwnerIds();

    private int GetCurrentUserId() =>
        int.TryParse(User.FindFirst("user_id")?.Value, out var id) ? id : 0;

    private async Task WriteAuditAsync(string action, string entityType, string? entityId, string? details = null)
    {
        try
        {
            _db.AdminApiAuditTrails.Add(new AdminApiAuditTrail
            {
                UserId = GetCurrentUserId(),
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Date = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit trail: {Action} {EntityType} {EntityId}", action, entityType, entityId);
        }
    }

    // ===== USERS =====

    // GET /v1/adminapi/users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        if (!User.HasPermission("read-user"))
            return Forbid();

        limit = Math.Min(limit, 1000);
        var users = await _userQueries.GetUsers();
        return Ok(users.Skip(offset).Take(limit).Select(MapUserResponse));
    }

    // POST /v1/adminapi/users
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminApiUserRequest request)
    {
        if (!User.HasPermission("create-user"))
            return Forbid();

        try
        {
            var user = await _userQueries.CreateUser(
                request.Username, request.Name, request.Email,
                request.Roles, request.Owners);

            _accessControl.InvalidateCache();
            await WriteAuditAsync("create", "user", user.Id.ToString());
            return Ok(MapUserResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", request.Username);
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // PUT /v1/adminapi/users/{user_id}
    [HttpPut("users/{user_id:int}")]
    public async Task<IActionResult> UpdateUser(int user_id, [FromBody] AdminApiUserRequest request)
    {
        if (!User.HasPermission("update-user"))
            return Forbid();

        var user = await _userQueries.UpdateUser(
            user_id, request.Name, request.Email, request.Roles, request.Owners);

        if (user == null)
            return NotFound(new ErrorResponse { ErrorMessage = "User not found" });

        _accessControl.InvalidateCache();
        await WriteAuditAsync("update", "user", user_id.ToString());
        return Ok(MapUserResponse(user));
    }

    // DELETE /v1/adminapi/users/{user_id}
    [HttpDelete("users/{user_id:int}")]
    public async Task<IActionResult> DeleteUser(int user_id)
    {
        if (!User.HasPermission("delete-user"))
            return Forbid();

        var deleted = await _userQueries.DeleteUser(user_id);
        if (!deleted)
            return NotFound(new ErrorResponse { ErrorMessage = "User not found" });

        _accessControl.InvalidateCache();
        await WriteAuditAsync("delete", "user", user_id.ToString());
        return Ok();
    }

    // GET /v1/adminapi/users/reference-data
    [HttpGet("users/reference-data")]
    public async Task<IActionResult> GetUsersReferenceData()
    {
        if (!User.HasPermission("read-user-reference-data"))
            return Forbid();

        var roles = await _userQueries.GetRoles();
        var owners = await _dictionaryQueries.GetOwners();

        return Ok(new AdminApiUsersReferenceDataDto
        {
            Roles = roles.Select(r => new AdminApiRoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Permissions = r.RolePermissions?.Select(rp =>
                    new ReferenceDataItem { Id = rp.PermissionId, Name = rp.Permission?.Name ?? "" }).ToList() ?? []
            }).ToList(),
            Owners = owners.Select(o => new ReferenceDataItem { Id = o.Id, Name = o.Name }).ToList()
        });
    }

    // GET /v1/adminapi/userinfo
    [HttpGet("userinfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        if (!User.HasPermission("read-user-info"))
            return Forbid();

        var languages = await _dictionaryQueries.GetLanguages();
        var owners = await _dictionaryQueries.GetOwnersWithProducts();
        var userOwnerIds = GetOwnerIds();

        return Ok(new AdminApiUserInfoDto
        {
            Languages = languages.Select(l => new LanguageInfoDto
            {
                Id = l.Id,
                Name = l.Description,
                FlagKey = l.FlagKey
            }).ToList(),
            Roles = User.FindAll("role").Select(c => c.Value).ToHashSet(),
            Permissions = User.GetPermissions(),
            Owners = owners
                .Where(o => userOwnerIds.Contains(o.Id))
                .Select(o => new OwnerWithProductsDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Products = o.Products?.Select(p => new ReferenceDataItem { Id = p.Id, Name = p.Name }).ToList() ?? []
                }).ToList()
        });
    }

    // ===== TEXTS =====

    // POST /v1/adminapi/texts/field
    [HttpPost("texts/field")]
    [RequireOwnerAccess]
    public async Task<IActionResult> CreateTextField([FromBody] AdminApiTextFieldRequest request)
    {
        if (!User.HasPermission("create-text-field"))
            return Forbid();

        await _translationQueries.UpsertAdminTranslations(
            request.OwnerId, request.ProductId, request.Language,
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            {
                [request.Page] = new()
                {
                    [request.Key] = new() { [request.Language] = request.Value }
                }
            });

        await WriteAuditAsync("create", "text-field", $"{request.OwnerId}/{request.Page}/{request.Key}");
        return Ok(new { modified = 1 });
    }

    // PUT /v1/adminapi/texts/field
    [HttpPut("texts/field")]
    [RequireOwnerAccess]
    public async Task<IActionResult> UpdateTextField([FromBody] AdminApiTextFieldRequest request)
    {
        if (!User.HasPermission("create-text") && !User.HasPermission("update-text"))
            return Forbid();

        await _translationQueries.UpsertAdminTranslations(
            request.OwnerId, request.ProductId, request.Language,
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>
            {
                [request.Page] = new()
                {
                    [request.Key] = new() { [request.Language] = request.Value }
                }
            });

        await WriteAuditAsync("update", "text-field", $"{request.OwnerId}/{request.Page}/{request.Key}");
        return Ok(new AdminApiTextFieldResponse { Value = request.Value });
    }

    // GET /v1/adminapi/texts
    [HttpGet("texts")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> GetTexts(
        [FromQuery] int owner_id,
        [FromQuery] int? product_id = null,
        [FromQuery] string language = "en")
    {
        if (!User.HasPermission("read-text"))
            return Forbid();

        var translations = await _translationQueries.GetLanguageTranslations(language, owner_id, product_id);
        return Ok(translations);
    }

    // ===== CONSENTS =====

    // GET /v1/adminapi/consents/reference-data
    [HttpGet("consents/reference-data")]
    public async Task<IActionResult> GetConsentReferenceData()
    {
        if (!User.HasPermission("read-consent-reference-data"))
            return Forbid();

        var consentTypes = await _dictionaryQueries.GetConsentTypes();
        var purposes = await _dictionaryQueries.GetPurposeCategories();
        var tags = await _dictionaryQueries.GetExpressionTags();
        var statuses = await _dictionaryQueries.GetExpressionStatuses();

        return Ok(new AdminApiReferenceDataDto
        {
            ConsentTypes = consentTypes.Select(t => new ReferenceDataItem { Id = t.Id, Name = t.Name }).ToList(),
            ConsentPurposes = purposes.Select(p => new ReferenceDataItem { Id = p.Id, Name = p.Name }).ToList(),
            ExpressionTags = tags.Select(t => new OwnedReferenceDataItem { Id = t.Id, Name = t.Name, OwnerId = t.OwnerId }).ToList(),
            ExpressionStatuses = statuses.Select(s => new ReferenceDataItem { Id = s.Id, Name = s.Name }).ToList()
        });
    }

    // GET /v1/adminapi/consents
    [HttpGet("consents")]
    public async Task<IActionResult> GetConsents(
        [FromQuery] bool group_channels = true,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        if (!User.HasPermission("read-consent"))
            return Forbid();

        limit = Math.Min(limit, 1000);
        var ownerIds = GetOwnerIds();
        var consents = await _consentQueries.GetConsents(ownerIds);

        var result = consents.Select(c => MapConsentResponse(c)).ToList();

        if (group_channels)
        {
            var groups = result.Where(c => c.IsGroup).ToList();
            var channels = result.Where(c => !c.IsGroup && c.ParentConsentId.HasValue).ToList();
            var standalone = result.Where(c => !c.IsGroup && !c.ParentConsentId.HasValue).ToList();

            foreach (var group in groups)
            {
                group.Channels = channels
                    .Where(ch => ch.ParentConsentId == group.Id)
                    .Cast<AdminApiConsentResponse>()
                    .ToList();
            }

            result = groups.Concat(standalone).ToList();
        }

        return Ok(result.Skip(offset).Take(limit));
    }

    // POST /v1/adminapi/consents
    [HttpPost("consents")]
    [RequireOwnerAccess]
    public async Task<IActionResult> CreateConsent([FromBody] AdminApiConsentRequest request)
    {
        if (!User.HasPermission("create-consent"))
            return Forbid();

        try
        {
            var consent = new PrivacyConsent.Data.Entities.Consent
            {
                Name = request.Name,
                Description = request.Description,
                OwnerId = request.OwnerId,
                PurposeId = request.PurposeId,
                ConsentTypeId = request.ConsentTypeId,
                SpecialDataCategoryId = request.SpecialDataCategoryId,
                DataSourceId = request.DataSourceId,
                ProcessingTypeId = request.ProcessingTypeId,
                ProductId = request.ProductId,
                HideByDefault = request.HideByDefault,
                ParentConsentId = request.ParentConsentId,
                IsGroup = request.IsGroup,
                ExpirationDate = request.ExpirationDate ?? DateTime.MaxValue,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            var created = await _consentQueries.CreateConsent(consent);
            await WriteAuditAsync("create", "consent", created.Id.ToString());
            return Ok(MapConsentResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create consent {Name}", request.Name);
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // GET /v1/adminapi/consents/{consent_id}
    [HttpGet("consents/{consent_id:int}")]
    public async Task<IActionResult> GetConsent(int consent_id)
    {
        if (!User.HasPermission("read-consent"))
            return Forbid();

        var consent = await _consentQueries.GetConsentById(consent_id);
        if (consent == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Consent not found" });

        return Ok(MapConsentResponse(consent));
    }

    // PUT /v1/adminapi/consents/{consent_id}
    [HttpPut("consents/{consent_id:int}")]
    [RequireOwnerAccess]
    public async Task<IActionResult> UpdateConsent(int consent_id, [FromBody] AdminApiConsentRequest request)
    {
        if (!User.HasPermission("update-consent"))
            return Forbid();

        var updated = new PrivacyConsent.Data.Entities.Consent
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            PurposeId = request.PurposeId,
            ConsentTypeId = request.ConsentTypeId,
            SpecialDataCategoryId = request.SpecialDataCategoryId,
            DataSourceId = request.DataSourceId,
            ProcessingTypeId = request.ProcessingTypeId,
            ProductId = request.ProductId,
            HideByDefault = request.HideByDefault,
            ParentConsentId = request.ParentConsentId,
            IsGroup = request.IsGroup,
            ExpirationDate = request.ExpirationDate ?? DateTime.MaxValue,
            ModifiedDate = DateTime.UtcNow
        };

        var result = await _consentQueries.UpdateConsent(consent_id, updated);
        if (result == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Consent not found" });

        await WriteAuditAsync("update", "consent", consent_id.ToString());
        return Ok(MapConsentResponse(result));
    }

    // DELETE /v1/adminapi/consents/{consent_id}
    [HttpDelete("consents/{consent_id:int}")]
    public async Task<IActionResult> DeleteConsent(int consent_id)
    {
        if (!User.HasPermission("update-consent"))
            return Forbid();

        var deleted = await _consentQueries.SoftDeleteConsent(consent_id);
        if (!deleted)
            return NotFound(new ErrorResponse { ErrorMessage = "Consent not found" });

        await WriteAuditAsync("delete", "consent", consent_id.ToString());
        return NoContent();
    }

    // ===== EXPRESSIONS =====

    // GET /v1/adminapi/consents/{consent_id}/expressions
    [HttpGet("consents/{consent_id:int}/expressions")]
    public async Task<IActionResult> GetExpressions(int consent_id)
    {
        if (!User.HasPermission("read-consent-expression"))
            return Forbid();

        var expressions = await _consentQueries.GetExpressionsByConsentId(consent_id);
        return Ok(expressions.Select(MapExpressionResponse));
    }

    // POST /v1/adminapi/consents/{consent_id}/expressions
    [HttpPost("consents/{consent_id:int}/expressions")]
    public async Task<IActionResult> CreateExpression(int consent_id, [FromBody] AdminApiExpressionRequest request)
    {
        if (!User.HasPermission("create-consent-expression"))
            return Forbid();

        try
        {
            var expression = new PrivacyConsent.Data.Entities.ConsentExpression
            {
                Name = request.Name,
                Description = request.Description,
                ConsentId = consent_id,
                StatusId = request.StatusId,
                IsDefault = request.IsDefault,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            var created = await _consentQueries.CreateExpression(expression);

            if (request.TagIds?.Count > 0)
                await _consentQueries.SetExpressionTags(created.Id, request.TagIds);

            if (request.Texts?.Count > 0)
            {
                foreach (var text in request.Texts)
                {
                    await _consentQueries.UpsertExpressionText(
                        created.Id, text.Language, text.Title, text.ShortText, text.LongText);
                }
            }

            // Reload with relationships
            var loaded = await _consentQueries.GetExpressionById(created.Id);
            await WriteAuditAsync("create", "expression", created.Id.ToString());
            return Ok(MapExpressionResponse(loaded!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create expression for consent {ConsentId}", consent_id);
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // GET /v1/adminapi/expressions/{expression_id}
    [HttpGet("expressions/{expression_id:int}")]
    public async Task<IActionResult> GetExpression(int expression_id)
    {
        if (!User.HasPermission("read-consent-expression"))
            return Forbid();

        var expression = await _consentQueries.GetExpressionById(expression_id);
        if (expression == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Expression not found" });

        return Ok(MapExpressionResponse(expression));
    }

    // PUT /v1/adminapi/expressions/{expression_id}
    [HttpPut("expressions/{expression_id:int}")]
    public async Task<IActionResult> UpdateExpression(int expression_id, [FromBody] AdminApiExpressionRequest request)
    {
        if (!User.HasPermission("update-consent-expression"))
            return Forbid();

        var updated = new PrivacyConsent.Data.Entities.ConsentExpression
        {
            Name = request.Name,
            Description = request.Description,
            ConsentId = request.ConsentId,
            StatusId = request.StatusId,
            IsDefault = request.IsDefault,
            ModifiedDate = DateTime.UtcNow
        };

        var result = await _consentQueries.UpdateExpression(expression_id, updated);
        if (result == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Expression not found" });

        if (request.TagIds != null)
            await _consentQueries.SetExpressionTags(expression_id, request.TagIds);

        if (request.Texts?.Count > 0)
        {
            foreach (var text in request.Texts)
            {
                await _consentQueries.UpsertExpressionText(
                    expression_id, text.Language, text.Title, text.ShortText, text.LongText);
            }
        }

        var loaded = await _consentQueries.GetExpressionById(expression_id);
        await WriteAuditAsync("update", "expression", expression_id.ToString());
        return Ok(MapExpressionResponse(loaded!));
    }

    // ===== EXPRESSION TEXTS =====

    // GET /v1/adminapi/expressions/{expression_id}/texts
    [HttpGet("expressions/{expression_id:int}/texts")]
    public async Task<IActionResult> GetExpressionTexts(int expression_id)
    {
        if (!User.HasPermission("read-consent-expression"))
            return Forbid();

        var expression = await _consentQueries.GetExpressionById(expression_id);
        if (expression == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Expression not found" });

        return Ok(expression.Texts?.Select(t => new AdminApiExpressionText
        {
            Language = t.Language,
            Title = t.Title,
            ShortText = t.ShortText,
            LongText = t.LongText
        }) ?? []);
    }

    // POST /v1/adminapi/expressions/{expression_id}/texts
    [HttpPost("expressions/{expression_id:int}/texts")]
    public async Task<IActionResult> CreateExpressionText(
        int expression_id,
        [FromBody] AdminApiExpressionText request)
    {
        if (!User.HasPermission("create-consent-expression-text"))
            return Forbid();

        await _consentQueries.UpsertExpressionText(
            expression_id, request.Language, request.Title, request.ShortText, request.LongText);

        await WriteAuditAsync("create", "expression-text", $"{expression_id}/{request.Language}");
        return Ok();
    }

    // PUT /v1/adminapi/expressions/{expression_id}/texts/{language}
    [HttpPut("expressions/{expression_id:int}/texts/{language}")]
    public async Task<IActionResult> UpdateExpressionText(
        int expression_id,
        string language,
        [FromBody] AdminApiExpressionText request)
    {
        if (!User.HasPermission("update-consent-expression-text-language"))
            return Forbid();

        await _consentQueries.UpsertExpressionText(
            expression_id, language, request.Title, request.ShortText, request.LongText);

        await WriteAuditAsync("update", "expression-text", $"{expression_id}/{language}");
        return Ok();
    }

    // ===== TAGS =====

    // GET /v1/adminapi/tags/{tag_id}
    [HttpGet("tags/{tag_id:int}")]
    public async Task<IActionResult> GetTag(int tag_id)
    {
        if (!User.HasPermission("read-consent-expression"))
            return Forbid();

        var tag = await _consentQueries.GetTagById(tag_id);
        if (tag == null || !tag.OwnerId.HasValue || !GetOwnerIds().Contains(tag.OwnerId.Value))
            return NotFound(new ErrorResponse { ErrorMessage = "Tag not found" });

        return Ok(new AdminApiTagResponse { Id = tag.Id, Name = tag.Name, OwnerId = tag.OwnerId ?? 0 });
    }

    // GET /v1/adminapi/tags
    [HttpGet("tags")]
    public async Task<IActionResult> GetTags()
    {
        if (!User.HasPermission("read-consent-expression"))
            return Forbid();

        var tags = await _consentQueries.GetTags(GetOwnerIds());
        return Ok(tags.Select(t => new AdminApiTagResponse { Id = t.Id, Name = t.Name, OwnerId = t.OwnerId ?? 0 }));
    }

    // POST /v1/adminapi/tags
    [HttpPost("tags")]
    [RequireOwnerAccess]
    public async Task<IActionResult> CreateTag([FromBody] AdminApiTagRequest request)
    {
        if (!User.HasPermission("create-consent-expression"))
            return Forbid();

        try
        {
            var tag = await _consentQueries.CreateTag(request.Name, request.OwnerId);
            await WriteAuditAsync("create", "tag", tag.Id.ToString());
            var locationUri = $"{Request.Scheme}://{Request.Host}{Request.Path}/{tag.Id}";

            return Created(locationUri,
                new AdminApiTagResponse { Id = tag.Id, Name = tag.Name, OwnerId = tag.OwnerId ?? 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tag {Name}", request.Name);
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // DELETE /v1/adminapi/tags
    [HttpDelete("tags")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> DeleteTag([FromQuery] int id, [FromQuery] int owner_id)
    {
        if (!User.HasPermission("delete-consent-expression"))
            return Forbid();

        var deleted = await _consentQueries.DeleteTag(id);
        if (!deleted)
            return NotFound(new ErrorResponse { ErrorMessage = "Tag not found" });

        await WriteAuditAsync("delete", "tag", id.ToString());
        return NoContent();
    }

    // ===== MAPPERS =====

    private static AdminApiUserResponse MapUserResponse(PrivacyConsent.Data.Entities.User user)
    {
        return new AdminApiUserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            Roles = user.UserRoles?.Select(ur =>
                new ReferenceDataItem { Id = ur.RoleId, Name = ur.Role?.Name ?? "" }).ToList() ?? [],
            Owners = user.UserOwners?.Select(uo =>
                new ReferenceDataItem { Id = uo.OwnerId, Name = uo.OwnerId.ToString() }).ToList() ?? []
        };
    }

    private static AdminApiConsentsListItemDto MapConsentResponse(PrivacyConsent.Data.Entities.Consent consent)
    {
        return new AdminApiConsentsListItemDto
        {
            Id = consent.Id,
            Name = consent.Name,
            Description = consent.Description,
            OwnerId = consent.OwnerId ?? 0,
            PurposeId = consent.PurposeId,
            ConsentTypeId = consent.ConsentTypeId,
            CreatedDate = consent.CreatedDate,
            ModifiedDate = consent.ModifiedDate,
            SpecialDataCategoryId = consent.SpecialDataCategoryId,
            DataSourceId = consent.DataSourceId,
            ProcessingTypeId = consent.ProcessingTypeId,
            ProductId = consent.ProductId,
            HideByDefault = consent.HideByDefault,
            ParentConsentId = consent.ParentConsentId,
            IsGroup = consent.IsGroup,
            ExpirationDate = consent.ExpirationDate,
            ConsentRank = consent.ConsentRank,
            DeleteAt = consent.DeleteAt
        };
    }

    private static AdminApiExpressionResponse MapExpressionResponse(PrivacyConsent.Data.Entities.ConsentExpression expression)
    {
        return new AdminApiExpressionResponse
        {
            Id = expression.Id,
            Name = expression.Name,
            Description = expression.Description,
            ConsentId = expression.ConsentId,
            CreatedDate = expression.CreatedDate,
            ModifiedDate = expression.ModifiedDate,
            StatusId = expression.StatusId,
            IsDefault = expression.IsDefault,
            Tags = expression.TagList?.Select(tl =>
                new ReferenceDataItem { Id = tl.ConsentExpressionTagId, Name = tl.Tag?.Name ?? "" }).ToList() ?? [],
            Texts = expression.Texts?.Select(t => new AdminApiExpressionText
            {
                Language = t.Language,
                Title = t.Title,
                ShortText = t.ShortText,
                LongText = t.LongText
            }).ToList() ?? []
        };
    }
}
