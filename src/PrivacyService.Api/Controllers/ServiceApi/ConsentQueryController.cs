using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyConsent.Infrastructure.Cache;
using PrivacyService.Api.Filters;
using PrivacyService.Api.Middleware;
using PrivacyService.Api.Services;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class ConsentQueryController : ControllerBase
{
    private readonly IConsentQueries _consentQueries;
    private readonly IExpressionQueries _expressionQueries;
    private readonly IUserConsentQueries _userConsentQueries;
    private readonly IMasterIdQueries _masterIdQueries;
    private readonly IRequestAttemptQueries _requestAttemptQueries;
    private readonly IConfiguration _config;

    public ConsentQueryController(
        IConsentQueries consentQueries,
        IExpressionQueries expressionQueries,
        IUserConsentQueries userConsentQueries,
        IMasterIdQueries masterIdQueries,
        IRequestAttemptQueries requestAttemptQueries,
        IConfiguration config)
    {
        _consentQueries = consentQueries;
        _expressionQueries = expressionQueries;
        _userConsentQueries = userConsentQueries;
        _masterIdQueries = masterIdQueries;
        _requestAttemptQueries = requestAttemptQueries;
        _config = config;
    }

    // POST /v1/serviceapi/decision-request-attempts
    [HttpPost("decision-request-attempts")]
    public async Task<IActionResult> PostDecisionRequestAttempt(
        [FromQuery] int consent_id,
        [FromQuery] string user_id,
        [FromQuery] int id_type_id,
        [FromQuery] string expression_tag,
        [FromQuery] bool exclude_positive = false,
        [FromQuery] bool exclude_negative = false,
        [FromQuery] int? user_consent_source_id = null,
        [FromQuery] string language = "en")
    {
        if (!User.HasOwnerAccess(await _consentQueries.GetConsentOwner(consent_id)))
            return Forbid();

        var expressions = await _expressionQueries.GetExpressionsByConsentId(
            consent_id, user_id, id_type_id, language, expression_tag);

        if (expressions.Count == 0)
            return Ok(new object[] { });

        var master = await _masterIdQueries.GetOrCreateMasterId(user_id, id_type_id);
        if (master != null)
        {
            var expr = expressions[0];
            await _requestAttemptQueries.RegisterRequestAttempt(
                master.Id, consent_id, expr.ConsentExpressionId,
                language, user_consent_source_id, user_id, id_type_id);
        }

        return Ok(expressions);
    }

    // GET /v1/serviceapi/decision-request-attempts
    [HttpGet("decision-request-attempts")]
    public async Task<IActionResult> GetDecisionRequestAttempt(
        [FromQuery] int consent_id,
        [FromQuery] string user_id,
        [FromQuery] int id_type_id,
        [FromQuery] string expression_tag,
        [FromQuery] string language = "en")
    {
        if (!User.HasOwnerAccess(await _consentQueries.GetConsentOwner(consent_id)))
            return Forbid();

        var expressions = await _expressionQueries.GetExpressionsByConsentId(
            consent_id, user_id, id_type_id, language, expression_tag);

        if (expressions.Count == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "Expression not found" });

        return Ok(expressions);
    }

    // POST /v1/serviceapi/dashboard/consents-list-grouped
    [HttpPost("dashboard/consents-list-grouped")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> GetConsentsListGrouped(
        [FromQuery] string user_id,
        [FromQuery] int id_type_id,
        [FromQuery] int owner_id,
        [FromQuery] string? expression_tag = null,
        [FromQuery] string language = "en")
    {

        var expressions = await _expressionQueries.GetRandExpressionsByProductId(
            owner_id, null, user_id, id_type_id, language, expression_tag);

        return Ok(expressions);
    }

    // GET /v1/serviceapi/user-decision-history
    [HttpGet("user-decision-history")]
    public async Task<IActionResult> GetUserDecisionHistory(
        [FromQuery] int consent_id,
        [FromQuery] string user_id,
        [FromQuery] int id_type_id)
    {
        if (!User.HasOwnerAccess(await _consentQueries.GetConsentOwner(consent_id)))
            return Forbid();

        var history = await _userConsentQueries.ReadDecisionHistory(user_id, id_type_id, consent_id);

        return Ok(history.Select(h => new DecisionHistoryItemDto
        {
            ConsentId = consent_id,
            ConsentExpressionId = h.ConsentExpressionId,
            ParentConsentExpressionId = h.ParentConsentExpressionId,
            PresentedLanguage = h.PresentedLanguage ?? "en",
            ChangeContext = h.ChangeContext != null
                ? JsonSerializer.Deserialize<JsonElement>(h.ChangeContext)
                : null,
            IsAgreed = h.IsAgreed,
            Date = h.Date,
            UserConsentSourceId = h.UserConsentSourceId
        }));
    }

    // POST /v1/serviceapi/user-consent-decisions-batch (short query)
    [HttpPost("user-consent-decisions-batch")]
    public async Task<IActionResult> GetUserConsentDecisionsShort(
        [FromBody] List<UserConsentDecisionUniqueDto> requests)
    {
        var tuples = requests.Select(r => (r.ConsentId, r.UserId, r.IdTypeId)).ToList();
        var results = await _userConsentQueries.GetUserConsentDecisionsShort(tuples);

        return Ok(results.Select(r => new UserConsentDecisionShortDto
        {
            ConsentId = r.ConsentId,
            UserId = r.UserId,
            IdTypeId = r.IdTypeId,
            IsAgreed = r.IsAgreed
        }));
    }

    // GET /v1/serviceapi/user-consent-decisions-batch (paginated)
    [HttpGet("user-consent-decisions-batch")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> GetUserConsentDecisionsBatch(
        [FromQuery] int owner_id,
        [FromQuery] int? consent_id = null,
        [FromQuery] int offset = 0)
    {

        var limit = _config.GetValue("QueryBatchLimit", 1000);
        var results = await _userConsentQueries.GetUserConsentDecisionsBatch(owner_id, consent_id, offset, limit);

        return Ok(new UserConsentDecisionBatchDto
        {
            Decisions = results.Select(r => new UserConsentDecisionBatchItemDto
            {
                ConsentId = r.ConsentId,
                UserId = r.UserId,
                IdTypeId = r.IdTypeId,
                Ids = r.Ids.Select(m => new UserIdDto { UserId = m.UserId, IdTypeId = m.IdTypeId }).ToList(),
                IsAgreed = r.IsAgreed,
                DecisionId = r.DecisionId,
                OwnerId = r.OwnerId,
                ChangeContext = r.ChangeContext != null
                    ? JsonSerializer.Deserialize<JsonElement>(r.ChangeContext)
                    : null,
                LastDecisionDate = r.LastDecisionDate,
                PresentedLanguage = r.PresentedLanguage,
                ConsentExpressionId = r.ConsentExpressionId,
                ParentConsentExpressionId = r.ParentConsentExpressionId,
                UserConsentSourceId = r.UserConsentSourceId,
                ConsentTypeId = r.ConsentTypeId
            }).ToList(),
            Offset = offset,
            Limit = limit
        });
    }
}
