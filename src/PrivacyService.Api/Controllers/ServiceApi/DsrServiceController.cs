using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Infrastructure.ExternalApis;
using PrivacyService.Api.Services;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class DsrServiceController : ControllerBase
{
    private readonly IDsrQueries _dsrQueries;
    private readonly IDataSubjectRightsService _dsrService;
    private readonly IZendeskClient _zendeskClient;
    private readonly IConfiguration _config;
    private readonly ILogger<DsrServiceController> _logger;

    public DsrServiceController(
        IDsrQueries dsrQueries,
        IDataSubjectRightsService dsrService,
        IZendeskClient zendeskClient,
        IConfiguration config,
        ILogger<DsrServiceController> logger)
    {
        _dsrQueries = dsrQueries;
        _dsrService = dsrService;
        _zendeskClient = zendeskClient;
        _config = config;
        _logger = logger;
    }

    // GET /v1/serviceapi/telenor-id-dsr/requests
    [HttpGet("telenor-id-dsr/requests")]
    public IActionResult GetTelenorIdDsrRequests(
        [FromQuery(Name = "user-id")] string userId,
        [FromQuery] string email)
    {
        var ownerIds = new List<int> { OwnerConstants.TdOwnerId };
        var dsrRights = _dsrService.CreateOwnerMap(ownerIds, new Dictionary<int, Dictionary<string, bool>>());
        return Ok(dsrRights);
    }

    // POST /v1/serviceapi/telenor-id-dsr/requests
    [HttpPost("telenor-id-dsr/requests")]
    public async Task<IActionResult> CreateTelenorIdDsrRequest(
        [FromQuery(Name = "user-id")] string userId,
        [FromQuery] string email,
        [FromQuery(Name = "dsr-type")] string dsrType)
    {
        var ticketId = await _dsrService.CreateDsrRequest(
            OwnerConstants.TdOwnerId, userId, IdTypeConstants.ConnectIdType,
            null, email, dsrType, null);

        if (ticketId == null)
            return StatusCode(500, new ErrorResponse { ErrorMessage = "Failed to create DSR request" });

        return Ok(new { message = "The request successfully processed.", ticket_id = ticketId });
    }

    // GET /v1/serviceapi/telenor-id-dsr/data-dump-links
    [HttpGet("telenor-id-dsr/data-dump-links")]
    public async Task<IActionResult> GetTelenorIdDataDumpLinks(
        [FromQuery(Name = "user-id")] string userId)
    {
        var ownerIds = new List<int> { OwnerConstants.TdOwnerId };
        var links = await _dsrService.GetPersonalDataLinks(
            ownerIds, userId, IdTypeConstants.ConnectIdType, null,
            _config.GetValue("DefaultLanguage", "en")!);

        return Ok(links);
    }

    // GET /v1/serviceapi/dsr/requests
    [HttpGet("dsr/requests")]
    public async Task<IActionResult> GetDsrRequests(
        [FromQuery] string type,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50)
    {
        limit = Math.Min(limit, _config.GetValue("QueryBatchLimit", 1000));
        var requests = await _dsrQueries.GetDsrRequests("", 0, type);
        return Ok(requests.Skip(offset).Take(limit));
    }

    // POST /v1/serviceapi/dsr/requests
    [HttpPost("dsr/requests")]
    public async Task<IActionResult> CreateDsrRequest(
        [FromQuery(Name = "user-id")] string userId,
        [FromQuery(Name = "ticket-id")] string? ticketId,
        [FromQuery] string type)
    {
        try
        {
            var effectiveTicketId = string.IsNullOrWhiteSpace(ticketId)
                ? $"auto-{Guid.NewGuid():N}"
                : ticketId;

            await _dsrQueries.CreateDsrRequest(effectiveTicketId, userId, IdTypeConstants.ConnectIdType, type, "open");
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create DSR request");
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // PATCH /v1/serviceapi/dsr/requests
    [HttpPatch("dsr/requests")]
    public async Task<IActionResult> UpdateDsrRequest(
        [FromQuery(Name = "user-id")] string userId,
        [FromQuery(Name = "ticket-id")] string? ticketId,
        [FromQuery] string type,
        [FromQuery] string status)
    {
        var effectiveTicketId = string.IsNullOrWhiteSpace(ticketId)
            ? $"auto-{Guid.NewGuid():N}"
            : ticketId;

        await _dsrQueries.UpdateDsrRequest(effectiveTicketId, userId, IdTypeConstants.ConnectIdType, type, status);
        return Ok();
    }

    // POST /v1/serviceapi/dsr/auto-deletion-request
    [HttpPost("dsr/auto-deletion-request")]
    public async Task<IActionResult> AutoDeletionRequest(
        [FromQuery(Name = "user-id")] string userId,
        [FromQuery] string status)
    {
        try
        {
            var ticketId = "auto-deletion";
            if (status == "open")
            {
                var existing = await _dsrQueries.GetDsrRequest(ticketId, userId, IdTypeConstants.ConnectIdType, "deletion");
                if (existing == null)
                    await _dsrQueries.CreateDsrRequest(ticketId, userId, IdTypeConstants.ConnectIdType, "deletion", status);
                else
                    await _dsrQueries.UpdateDsrRequest(ticketId, userId, IdTypeConstants.ConnectIdType, "deletion", status);
            }
            else
            {
                await _dsrQueries.UpdateDsrRequest(ticketId, userId, IdTypeConstants.ConnectIdType, "deletion", status);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process auto-deletion request");
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }
}
