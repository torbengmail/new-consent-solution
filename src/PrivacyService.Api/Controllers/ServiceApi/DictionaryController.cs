using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyService.Api.Middleware;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class DictionaryController : ControllerBase
{
    private readonly IDictionaryQueries _dictionaryQueries;
    private readonly ITranslationQueries _translationQueries;
    private readonly IConsentQueries _consentQueries;
    private readonly IExpressionQueries _expressionQueries;
    private readonly IConfiguration _config;

    public DictionaryController(
        IDictionaryQueries dictionaryQueries,
        ITranslationQueries translationQueries,
        IConsentQueries consentQueries,
        IExpressionQueries expressionQueries,
        IConfiguration config)
    {
        _dictionaryQueries = dictionaryQueries;
        _translationQueries = translationQueries;
        _consentQueries = consentQueries;
        _expressionQueries = expressionQueries;
        _config = config;
    }

    // GET /v1/serviceapi/dictionaries/consent-types
    [HttpGet("dictionaries/consent-types")]
    public async Task<IActionResult> GetConsentTypes() =>
        Ok((await _dictionaryQueries.GetConsentTypes())
            .Select(t => new ReferenceDataItem { Id = t.Id, Name = t.Name }));

    // GET /v1/serviceapi/dictionaries/consent-purposes
    [HttpGet("dictionaries/consent-purposes")]
    public async Task<IActionResult> GetConsentPurposes() =>
        Ok((await _dictionaryQueries.GetPurposeCategories())
            .Select(p => new ReferenceDataItem { Id = p.Id, Name = p.Name }));

    // GET /v1/serviceapi/dictionaries/expression-tags
    [HttpGet("dictionaries/expression-tags")]
    public async Task<IActionResult> GetExpressionTags([FromQuery] int? owner_id = null) =>
        Ok((await _dictionaryQueries.GetExpressionTags())
            .Where(t => owner_id == null || t.OwnerId == owner_id)
            .Select(t => new OwnedReferenceDataItem { Id = t.Id, Name = t.Name, OwnerId = t.OwnerId }));

    // GET /v1/serviceapi/dictionaries/expression-statuses
    [HttpGet("dictionaries/expression-statuses")]
    public async Task<IActionResult> GetExpressionStatuses() =>
        Ok((await _dictionaryQueries.GetExpressionStatuses())
            .Select(s => new ReferenceDataItem { Id = s.Id, Name = s.Name }));

    // GET /v1/serviceapi/dictionaries/languages
    [HttpGet("dictionaries/languages")]
    public async Task<IActionResult> GetLanguages() =>
        Ok((await _dictionaryQueries.GetLanguages())
            .Select(l => new LanguageDto { Name = l.Id, Description = l.Description, FlagKey = l.FlagKey }));

    // GET /v1/serviceapi/dictionaries/id-types
    [HttpGet("dictionaries/id-types")]
    public async Task<IActionResult> GetIdTypes() =>
        Ok((await _dictionaryQueries.GetIdTypes())
            .Select(t => new ReferenceDataItem { Id = t.Id, Name = t.Name }));

    // GET /v1/serviceapi/dictionaries/owners
    [HttpGet("dictionaries/owners")]
    public async Task<IActionResult> GetOwners() =>
        Ok((await _dictionaryQueries.GetOwners())
            .Select(o => new ReferenceDataItem { Id = o.Id, Name = o.Name }));

    // GET /v1/serviceapi/dictionaries/products
    [HttpGet("dictionaries/products")]
    public async Task<IActionResult> GetProducts() =>
        Ok((await _dictionaryQueries.GetProducts())
            .Select(p => new { id = p.Id, name = p.Name, owner_id = (int?)p.OwnerId }));

    // GET /v1/serviceapi/dictionaries/owners-products
    [HttpGet("dictionaries/owners-products")]
    public async Task<IActionResult> GetOwnersWithProducts()
    {
        var owners = await _dictionaryQueries.GetOwnersWithProducts();
        return Ok(owners.Select(o => new
        {
            id = o.Id,
            name = o.Name,
            products = o.Products?.Select(p => new ReferenceDataItem { Id = p.Id, Name = p.Name }) ?? []
        }));
    }

    // GET /v1/serviceapi/texts
    [HttpGet("texts")]
    public async Task<IActionResult> GetTexts(
        [FromQuery] string? language = null,
        [FromQuery] int? owner_id = null,
        [FromQuery] int? product_id = null,
        [FromQuery] bool augmented = true)
    {
        var lang = language ?? _config.GetValue("DefaultLanguage", "en")!;
        if (owner_id == null)
            return BadRequest(new ErrorResponse { ErrorMessage = "owner_id is required" });

        var translations = await _translationQueries.GetLanguageTranslations(lang, owner_id.Value, product_id);
        return Ok(translations);
    }

    // GET /v1/serviceapi/texts/consent-expression
    [HttpGet("texts/consent-expression")]
    public async Task<IActionResult> GetConsentExpressionText(
        [FromQuery] int consent_id,
        [FromQuery] string expression_tag,
        [FromQuery] string language = "en")
    {
        if (!User.HasOwnerAccess(await _consentQueries.GetConsentOwner(consent_id)))
            return Forbid();

        var expressions = await _expressionQueries.GetExpressionsByConsentId(
            consent_id, "", 0, language, expression_tag);

        if (expressions.Count == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "Consent expression text not found" });

        return Ok(expressions[0]);
    }

    // GET /v1/serviceapi/users/user-info
    [HttpGet("users/user-info")]
    public IActionResult GetUserInfo()
    {
        var owners = User.GetOwnerIds();
        var permissions = User.GetPermissions();
        return Ok(new
        {
            username = User.Identity?.Name,
            permissions,
            owners
        });
    }

    // GET /v1/serviceapi/consents
    [HttpGet("consents")]
    public async Task<IActionResult> GetConsents(
        [FromQuery] int? use_case_id = null,
        [FromQuery] int? owner_id = null)
    {
        if (use_case_id == null && owner_id == null)
            return BadRequest(new ErrorResponse { ErrorMessage = "Either use_case_id or owner_id is required" });

        var rawConsents = owner_id.HasValue && use_case_id.HasValue
            ? await _consentQueries.GetConsentsByUseCase(owner_id.Value, use_case_id.Value)
            : await _consentQueries.GetConsents(owner_id);

        var consents = rawConsents.Select(c => new PrivacyConsent.Domain.DTOs.ServiceApi.ConsentDto
        {
            ConsentId = c.ConsentId,
            Name = c.Name,
            DefaultOptIn = c.DefaultOptIn,
            ConsentType = c.ConsentType,
            ConsentTypeName = c.ConsentTypeName
        }).ToList();

        return Ok(new ConsentsResponse { Consents = consents });
    }

    // GET /v1/serviceapi/consents/{consent_id}
    [HttpGet("consents/{consent_id:int}")]
    public async Task<IActionResult> GetConsentById(int consent_id)
    {
        var consent = await _consentQueries.GetConsentById(consent_id);
        if (consent == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Consent not found" });

        return Ok(consent);
    }
}
