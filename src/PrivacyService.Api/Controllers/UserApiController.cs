using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyConsent.Domain.DTOs.UserApi;
using PrivacyConsent.Infrastructure.Cache;
using PrivacyService.Api.Middleware;
using PrivacyService.Api.Services;

namespace PrivacyService.Api.Controllers;

[ApiController]
[Route("v1/userapi")]
[Authorize]
public class UserApiController : ControllerBase
{
    private readonly IExpressionQueries _expressionQueries;
    private readonly IDictionaryQueries _dictionaryQueries;
    private readonly ITranslationQueries _translationQueries;
    private readonly UserDataCacheService _cacheService;
    private readonly IDashboardService _dashboardService;
    private readonly IDecisionService _decisionService;
    private readonly IDataSubjectRightsService _dsrService;
    private readonly IConfiguration _config;
    private readonly ILogger<UserApiController> _logger;

    public UserApiController(
        IExpressionQueries expressionQueries,
        IDictionaryQueries dictionaryQueries,
        ITranslationQueries translationQueries,
        UserDataCacheService cacheService,
        IDashboardService dashboardService,
        IDecisionService decisionService,
        IDataSubjectRightsService dsrService,
        IConfiguration config,
        ILogger<UserApiController> logger)
    {
        _expressionQueries = expressionQueries;
        _dictionaryQueries = dictionaryQueries;
        _translationQueries = translationQueries;
        _cacheService = cacheService;
        _dashboardService = dashboardService;
        _decisionService = decisionService;
        _dsrService = dsrService;
        _config = config;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst("sub")?.Value ?? User.Identity?.Name ?? "";
    private string? GetAccessToken() => Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
    private string? GetEmail() => User.FindFirst("email")?.Value;

    // POST /v1/userapi/dashboard/decisions-list-grouped
    [HttpPost("dashboard/decisions-list-grouped")]
    public async Task<IActionResult> GetDecisionsListGrouped(
        [FromQuery] int owner_id,
        [FromQuery] bool adjust_by_context = false,
        [FromQuery] string? language = null)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        var userId = GetUserId();
        var accessToken = GetAccessToken();

        var expressions = await _expressionQueries.GetRandExpressionsByProductId(
            owner_id, null, userId, IdTypeConstants.ConnectIdType, lang, null);

        return Ok(expressions);
    }

    // GET /v1/userapi/dsr-requests
    [HttpGet("dsr-requests")]
    public async Task<IActionResult> GetDsrRequests()
    {
        var userId = GetUserId();
        var accessToken = GetAccessToken();

        var ownerIds = await _dsrService.GetOwnerIds(userId, IdTypeConstants.ConnectIdType, accessToken);

        // Get cached DSR status per owner
        var cacheKeys = ownerIds.SelectMany(ownerId =>
            DsrConstants.GetDsrTypes(ownerId).Select(t => CacheConstants.CreateCompositeKey(ownerId, t)))
            .ToList();

        var cachedValues = await _cacheService.GetValuesInByOwner(
            userId, IdTypeConstants.ConnectIdType, cacheKeys, CacheConstants.DefaultRetentionHours);

        var castValues = cachedValues.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.ToDictionary(
                inner => inner.Key,
                inner => inner.Value != null));

        var dsrRights = _dsrService.CreateOwnerMap(ownerIds, castValues);

        return Ok(dsrRights.OrderByDescending(r => r.OwnerId));
    }

    // POST /v1/userapi/data-subject-rights
    [HttpPost("data-subject-rights")]
    public async Task<IActionResult> CreateDataSubjectRequest(
        [FromBody] DataSubjectRightsRequestDto request)
    {
        var userId = GetUserId();
        var email = GetEmail();
        var accessToken = GetAccessToken();

        if (string.IsNullOrEmpty(email))
            return BadRequest(new ErrorResponse { ErrorMessage = "Email is required" });

        // Sanitize note (HTML-encode to prevent XSS)
        var note = request.Note != null
            ? WebUtility.HtmlEncode(request.Note)
            : null;

        var ownerId = request.OwnerId ?? OwnerConstants.TdOwnerId;

        var ticketId = await _dsrService.CreateDsrRequest(
            ownerId, userId, IdTypeConstants.ConnectIdType,
            accessToken, email, request.Right, note);

        if (ticketId == null)
            return StatusCode(500, new ErrorResponse { ErrorMessage = "Failed to create DSR request" });

        return Ok(new DsrRequestCreatedResponse { Message = "The request successfully processed.", TicketId = ticketId });
    }

    // GET /v1/userapi/dsr-texts
    [HttpGet("dsr-texts")]
    public async Task<IActionResult> GetDsrTexts(
        [FromQuery] string? language = null,
        [FromQuery] bool augmented = true)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        var userId = GetUserId();
        var accessToken = GetAccessToken();

        var ownerIds = await _dsrService.GetOwnerIds(userId, IdTypeConstants.ConnectIdType, accessToken);

        var texts = await _translationQueries.GetLanguageTranslationsMultiOwners(
            lang, ownerIds, "dsr");

        return Ok(texts.Select(t => new MultiOwnerTextDto
        {
            OwnerId = t.OwnerId,
            OwnerName = t.OwnerName,
            Text = t.Translations != null
                ? TranslationQueries.FlattenTranslationJson(t.Translations)
                : null
        }));
    }

    // GET /v1/userapi/legal-texts
    [HttpGet("legal-texts")]
    public async Task<IActionResult> GetLegalTexts(
        [FromQuery] string? language = null,
        [FromQuery] bool augmented = true)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        var userId = GetUserId();
        var accessToken = GetAccessToken();

        var ownerIds = await _dsrService.GetOwnerIds(userId, IdTypeConstants.ConnectIdType, accessToken);

        var texts = await _translationQueries.GetLanguageTranslationsMultiOwners(
            lang, ownerIds, "legal");

        return Ok(texts.Select(t => new MultiOwnerTextDto
        {
            OwnerId = t.OwnerId,
            OwnerName = t.OwnerName,
            Text = t.Translations != null
                ? TranslationQueries.FlattenTranslationJson(t.Translations)
                : null
        }));
    }

    // GET /v1/userapi/pd-dump-links-extended
    [HttpGet("pd-dump-links-extended")]
    public async Task<IActionResult> GetPersonalDataDumpLinks(
        [FromQuery] string? language = null)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        var userId = GetUserId();
        var accessToken = GetAccessToken();

        var ownerIds = await _dsrService.GetOwnerIds(userId, IdTypeConstants.ConnectIdType, accessToken);
        var links = await _dsrService.GetPersonalDataLinks(
            ownerIds, userId, IdTypeConstants.ConnectIdType, accessToken, lang);

        return Ok(links);
    }

    // POST /v1/userapi/user-consent-decisions (get random expressions)
    [HttpPost("user-consent-decisions")]
    public async Task<IActionResult> GetRandomExpressions(
        [FromQuery] int owner_id,
        [FromQuery] int? product_id,
        [FromQuery] string expression_tag,
        [FromQuery] string language = "en")
    {
        var userId = GetUserId();

        var expressions = await _expressionQueries.GetRandExpressionsByProductId(
            owner_id, product_id, userId, IdTypeConstants.ConnectIdType, language, expression_tag);

        if (expressions.Count == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "No consent expressions found" });

        return Ok(expressions);
    }

    // PUT /v1/userapi/user-consent-decisions (save decisions)
    [HttpPut("user-consent-decisions")]
    public async Task<IActionResult> SaveDecisions(
        [FromBody] List<UserConsentDecisionForUserApiRequest> decisions)
    {
        if (decisions.Count == 0)
            return BadRequest(new ErrorResponse { ErrorMessage = "Decisions list cannot be empty" });

        var userId = GetUserId();

        try
        {
            var inputs = decisions.Select(d => new DecisionInput
            {
                ConsentExpressionId = d.ConsentExpressionId,
                ParentConsentExpressionId = d.ParentConsentExpressionId,
                IsAgreed = d.IsAgreed,
                UserConsentSourceId = d.UserConsentSourceId,
                PresentedLanguage = d.PresentedLanguage,
                ChangeContext = d.ChangeContext?.GetRawText()
            }).ToList();

            var auditIds = await _decisionService.SaveDecisions(inputs, userId, IdTypeConstants.ConnectIdType);
            return Ok(auditIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user decisions");
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // GET /v1/userapi/languages
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages()
    {
        var languages = await _dictionaryQueries.GetLanguages();
        return Ok(languages.Select(l => l.Id));
    }

    // GET /v1/userapi/ui-settings
    [HttpGet("ui-settings")]
    public async Task<IActionResult> GetUiSettings(
        [FromQuery] string? language = null,
        [FromQuery] bool augmented = true,
        [FromQuery] int? referrer = null)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        var effectiveReferrer = referrer ?? _config.GetValue("DefaultUserConsentSource", 1);

        // Get translations for default owner
        var translations = await _translationQueries.GetLanguageTranslations(
            lang, OwnerConstants.TdOwnerId, null);

        return Ok(new TranslationAndSkinDto
        {
            Texts = translations,
            Theme = new SkinThemeDto
            {
                OwnerId = OwnerConstants.TdOwnerId,
                Referrer = effectiveReferrer
            }
        });
    }
}
