using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyService.Api.Filters;
using PrivacyService.Api.Middleware;
using PrivacyService.Api.Services;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class ConsentDecisionController : ControllerBase
{
    private readonly IExpressionQueries _expressionQueries;
    private readonly IUserConsentQueries _userConsentQueries;
    private readonly IDecisionService _decisionService;
    private readonly IConfiguration _config;
    private readonly ILogger<ConsentDecisionController> _logger;

    public ConsentDecisionController(
        IExpressionQueries expressionQueries,
        IUserConsentQueries userConsentQueries,
        IDecisionService decisionService,
        IConfiguration config,
        ILogger<ConsentDecisionController> logger)
    {
        _expressionQueries = expressionQueries;
        _userConsentQueries = userConsentQueries;
        _decisionService = decisionService;
        _config = config;
        _logger = logger;
    }

    // POST /v1/serviceapi/user-consent-decisions (get random expressions by product)
    [HttpPost("user-consent-decisions")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> GetRandomConsentsByProduct(
        [FromQuery] int owner_id,
        [FromQuery] int? product_id,
        [FromQuery] string user_id,
        [FromQuery] int id_type_id,
        [FromQuery] string expression_tag,
        [FromQuery] string language = "en")
    {

        var expressions = await _expressionQueries.GetRandExpressionsByProductId(
            owner_id, product_id, user_id, id_type_id, language, expression_tag);

        if (expressions.Count == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "No consent expressions found" });

        return Ok(expressions);
    }

    // PUT /v1/serviceapi/user-consent-decisions (save decisions)
    [HttpPut("user-consent-decisions")]
    public async Task<IActionResult> SaveDecisions([FromBody] List<UserConsentDecisionRequest> decisions)
    {
        if (decisions.Count == 0)
            return BadRequest(new ErrorResponse { ErrorMessage = "Decisions list cannot be empty" });

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

            var first = decisions[0];
            var auditIds = await _decisionService.SaveDecisions(inputs, first.UserId, first.IdTypeId);
            return Ok(auditIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save decisions");
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // PATCH /v1/serviceapi/retract-last-user-consent-decision
    [HttpPatch("retract-last-user-consent-decision")]
    public async Task<IActionResult> RetractLastDecision(
        [FromBody] RetractLastUserConsentDecisionRequest request)
    {
        var retracted = await _userConsentQueries.RetractLastDecision(
            request.UserId, request.IdTypeId, request.ConsentId, request.UserConsentSourceId);

        if (retracted == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "Decision not found" });

        return Ok();
    }

    // PATCH /v1/serviceapi/update-last-user-consent-decision
    [HttpPatch("update-last-user-consent-decision")]
    public async Task<IActionResult> UpdateLastDecision(
        [FromBody] UpdateLastUserConsentDecisionRequest request)
    {
        var updated = await _userConsentQueries.UpdateLastDecision(
            request.UserId, request.IdTypeId, request.ConsentId,
            request.UserConsentSourceId, request.Value);

        if (updated == 0)
            return NotFound(new ErrorResponse { ErrorMessage = "Decision not found" });

        return Ok();
    }
}
