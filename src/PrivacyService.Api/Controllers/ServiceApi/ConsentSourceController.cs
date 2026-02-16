using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyService.Api.Filters;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class ConsentSourceController : ControllerBase
{
    private readonly IUserConsentSourceQueries _sourceQueries;

    public ConsentSourceController(IUserConsentSourceQueries sourceQueries)
    {
        _sourceQueries = sourceQueries;
    }

    // POST /v1/serviceapi/user-consent-sources
    [HttpPost("user-consent-sources")]
    [RequireOwnerAccess]
    public async Task<IActionResult> CreateUserConsentSource([FromBody] UserConsentSourceRequest request)
    {
        var source = await _sourceQueries.CreateSource(
            request.Name, request.Description, request.UserConsentSourceTypeId,
            request.OwnerId, request.ProductId);

        return CreatedAtAction(nameof(GetUserConsentSources),
            new { id = source.Id, owner_id = source.OwnerId },
            new UserConsentSourceDto
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description ?? "",
                UserConsentSourceTypeId = source.UserConsentSourceTypeId,
                OwnerId = source.OwnerId ?? 0,
                ProductId = source.ProductId
            });
    }

    // GET /v1/serviceapi/user-consent-sources
    [HttpGet("user-consent-sources")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> GetUserConsentSources(
        [FromQuery] int owner_id,
        [FromQuery] int? id = null)
    {
        var sources = await _sourceQueries.GetSources(owner_id);
        if (id.HasValue)
            sources = sources.Where(s => s.Id == id.Value).ToList();

        return Ok(sources.Select(s => new UserConsentSourceDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description ?? "",
            UserConsentSourceTypeId = s.UserConsentSourceTypeId,
            OwnerId = s.OwnerId ?? 0,
            ProductId = s.ProductId
        }));
    }

    // PUT /v1/serviceapi/user-consent-sources
    [HttpPut("user-consent-sources")]
    [RequireOwnerAccess]
    public async Task<IActionResult> UpdateUserConsentSource([FromBody] UserConsentSourceDto request)
    {
        var source = await _sourceQueries.UpdateSource(
            request.Id, request.Name, request.Description,
            (int?)request.UserConsentSourceTypeId, request.OwnerId, request.ProductId);

        if (source == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Source not found" });

        return Ok(new UserConsentSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description ?? "",
            UserConsentSourceTypeId = source.UserConsentSourceTypeId,
            OwnerId = source.OwnerId ?? 0,
            ProductId = source.ProductId
        });
    }

    // PATCH /v1/serviceapi/user-consent-sources
    [HttpPatch("user-consent-sources")]
    [RequireOwnerAccess]
    public async Task<IActionResult> PatchUserConsentSource([FromBody] UserConsentSourcePatchRequest request)
    {
        var source = await _sourceQueries.UpdateSource(
            request.Id, request.Name, request.Description,
            request.UserConsentSourceTypeId, request.OwnerId, request.ProductId);

        if (source == null)
            return NotFound(new ErrorResponse { ErrorMessage = "Source not found" });

        return Ok(new UserConsentSourceDto
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description ?? "",
            UserConsentSourceTypeId = source.UserConsentSourceTypeId,
            OwnerId = source.OwnerId ?? 0,
            ProductId = source.ProductId
        });
    }

    // DELETE /v1/serviceapi/user-consent-sources
    [HttpDelete("user-consent-sources")]
    [RequireOwnerAccess("owner_id")]
    public async Task<IActionResult> DeleteUserConsentSource(
        [FromQuery] int id,
        [FromQuery] int owner_id)
    {
        var deleted = await _sourceQueries.DeleteSource(id);
        if (!deleted)
            return NotFound(new ErrorResponse { ErrorMessage = "Source not found" });

        return NoContent();
    }
}
