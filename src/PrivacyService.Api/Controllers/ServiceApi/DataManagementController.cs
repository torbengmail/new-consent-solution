using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyConsent.Infrastructure.Storage;
using PrivacyService.Api.Helpers;
using PrivacyService.Api.Middleware;

namespace PrivacyService.Api.Controllers.ServiceApi;

[ApiController]
[Route("v1/serviceapi")]
[Authorize(AuthenticationSchemes = "Basic")]
public class DataManagementController : ControllerBase
{
    private readonly IDataDumpQueries _dataDumpQueries;
    private readonly IMasterIdQueries _masterIdQueries;
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _config;
    private readonly ILogger<DataManagementController> _logger;

    public DataManagementController(
        IDataDumpQueries dataDumpQueries,
        IMasterIdQueries masterIdQueries,
        IFileStorageService fileStorage,
        IConfiguration config,
        ILogger<DataManagementController> logger)
    {
        _dataDumpQueries = dataDumpQueries;
        _masterIdQueries = masterIdQueries;
        _fileStorage = fileStorage;
        _config = config;
        _logger = logger;
    }

    // GET /v1/serviceapi/user-data-dump-json
    [HttpGet("user-data-dump-json")]
    public async Task<IActionResult> GetUserDataDumpJson(
        [FromQuery] string user_id,
        [FromQuery] int id_type_id)
    {
        if (!User.HasPermission("read-data-dump"))
            return Forbid();

        var decisions = await _dataDumpQueries.GetUserDecisionDataRecords(user_id, id_type_id);
        var attempts = await _dataDumpQueries.GetUserRequestAttemptDataRecords(user_id, id_type_id);

        return Ok(new DataDumpDto
        {
            Decisions = decisions.Select(d => new DataDumpDecisionItemDto
            {
                SourceId = d.SourceId,
                SourceName = d.SourceName,
                ConsentId = d.ConsentId,
                ExpressionId = d.ExpressionId,
                ExpressionTitle = d.ExpressionTitle,
                ExpressionText = d.ExpressionText,
                ExpressionLegal = d.ExpressionLegal,
                PresentedLanguage = d.PresentedLanguage,
                Date = d.Date,
                IsAgreed = d.IsAgreed,
                ChangeContext = d.ChangeContext
            }).ToList(),
            RequestAttempts = attempts.Select(a => new DataDumpRequestAttemptItemDto
            {
                SourceId = a.SourceId,
                SourceName = a.SourceName,
                ConsentId = a.ConsentId,
                ExpressionId = a.ExpressionId,
                ExpressionTitle = a.ExpressionTitle,
                ExpressionText = a.ExpressionText,
                ExpressionLegal = a.ExpressionLegal,
                PresentedLanguage = a.PresentedLanguage,
                Date = a.Date
            }).ToList()
        });
    }

    // GET /v1/serviceapi/user-data-dump-csv
    [HttpGet("user-data-dump-csv")]
    public async Task<IActionResult> GetUserDataDumpCsv(
        [FromQuery] string user_id,
        [FromQuery] int id_type_id)
    {
        if (!User.HasPermission("read-data-dump"))
            return Forbid();

        var decisions = await _dataDumpQueries.GetUserDecisionDataRecords(user_id, id_type_id);
        var attempts = await _dataDumpQueries.GetUserRequestAttemptDataRecords(user_id, id_type_id);

        var sb = new StringBuilder();
        sb.AppendLine("type,source_id,source_name,consent_id,expression_id,title,text,legal,language,date,is_agreed");

        foreach (var d in decisions)
        {
            sb.AppendLine($"decision,{d.SourceId},{Csv(d.SourceName)},{d.ConsentId},{d.ExpressionId}," +
                $"{Csv(d.ExpressionTitle)},{Csv(d.ExpressionText)},{Csv(d.ExpressionLegal)},{d.PresentedLanguage}," +
                $"{d.Date:O},{d.IsAgreed}");
        }

        foreach (var a in attempts)
        {
            sb.AppendLine($"request_attempt,{a.SourceId},{Csv(a.SourceName)},{a.ConsentId},{a.ExpressionId}," +
                $"{Csv(a.ExpressionTitle)},{Csv(a.ExpressionText)},{Csv(a.ExpressionLegal)},{a.PresentedLanguage}," +
                $"{a.Date:O},");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "user-data-dump.csv");
    }

    // PUT /v1/serviceapi/file-upload
    [HttpPut("file-upload")]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromQuery] string user_id,
        [FromQuery] string? ticket_id,
        [FromQuery] string product_id,
        [FromQuery] string file_name)
    {
        try
        {
            var effectiveTicketId = string.IsNullOrWhiteSpace(ticket_id)
                ? $"auto-{Guid.NewGuid():N}"
                : ticket_id;

            var safeUserId = PathSanitizer.SanitizeSegment(user_id);
            var safeTicketId = PathSanitizer.SanitizeSegment(effectiveTicketId);
            var safeProductId = PathSanitizer.SanitizeSegment(product_id);
            var safeFileName = PathSanitizer.SanitizeFileName(file_name);

            var path = $"personal-data/{safeUserId}/{safeTicketId}/{safeProductId}/{safeFileName}";

            var bucketName = _config.GetValue<string>("Storage:BucketName") ?? "privacy-personal-data";
            using var stream = file.OpenReadStream();
            await _fileStorage.UploadFileAsync(bucketName, path, stream, file.ContentType ?? "application/octet-stream");

            return Ok(new { path, size = file.Length });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File upload failed");
            return StatusCode(500, new ErrorResponse { ErrorMessage = "File upload failed" });
        }
    }

    // GET /v1/serviceapi/file-upload-link
    [HttpGet("file-upload-link")]
    public async Task<IActionResult> GetFileUploadLink(
        [FromQuery] string user_id,
        [FromQuery] string? ticket_id,
        [FromQuery] string product_id,
        [FromQuery] string file_name)
    {
        try
        {
            var effectiveTicketId = string.IsNullOrWhiteSpace(ticket_id)
                ? $"auto-{Guid.NewGuid():N}"
                : ticket_id;

            var safeUserId = PathSanitizer.SanitizeSegment(user_id);
            var safeTicketId = PathSanitizer.SanitizeSegment(effectiveTicketId);
            var safeProductId = PathSanitizer.SanitizeSegment(product_id);
            var safeFileName = PathSanitizer.SanitizeFileName(file_name);

            var path = $"personal-data/{safeUserId}/{safeTicketId}/{safeProductId}/{safeFileName}";
            var bucketName = _config.GetValue<string>("Storage:BucketName") ?? "privacy-personal-data";
            var link = await _fileStorage.GenerateUploadLinkAsync(bucketName, path, TimeSpan.FromMinutes(30));

            return Ok(new PersonalDataUploadLinkDto { S3Link = link });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload link");
            return StatusCode(500, new ErrorResponse { ErrorMessage = "Failed to generate upload link" });
        }
    }

    // POST /v1/serviceapi/id-type
    [HttpPost("id-type")]
    public async Task<IActionResult> CreateIdType([FromBody] IdTypeRequest request)
    {
        if (!User.HasPermission("create-id-type"))
            return Forbid();

        var idType = await _masterIdQueries.CreateIdType(request.Name);
        return Ok(new IdTypeResponse { Id = idType.Id, Name = idType.Name });
    }

    // POST /v1/serviceapi/id-mapping
    [HttpPost("id-mapping")]
    public async Task<IActionResult> CreateIdMapping([FromBody] IdMappingRequest request)
    {
        if (!User.HasPermission("create-id-mapping"))
            return Forbid();

        try
        {
            var mapping = await _masterIdQueries.CreateIdMapping(request.IdTypeId, request.Name);
            return Created($"/v1/serviceapi/id-mapping/{mapping.IdTypeId}",
                new IdMappingResponse { IdTypeId = mapping.IdTypeId, Name = mapping.Name });
        }
        catch (Exception ex)
        {
            return BadRequest(new ErrorResponse { ErrorMessage = ex.Message });
        }
    }

    // DELETE /v1/serviceapi/test-user
    [HttpDelete("test-user")]
    public async Task<IActionResult> DeleteTestUser([FromQuery] string test_user_group)
    {
        if (!User.HasPermission("delete-test-user"))
            return Forbid();

        var deleted = await _masterIdQueries.DeleteTestUser(test_user_group, IdTypeConstants.ConnectIdType);
        return Ok(new DeleteTestUserResponse { RequestAttempt = 0, UserConsent = deleted });
    }

    private static string Csv(string? value)
    {
        if (value == null) return "";
        var sanitized = value.Replace("\r", " ").Replace("\n", " ");
        if (sanitized.Length > 0 && "=+@-\t".IndexOf(sanitized[0]) >= 0)
            sanitized = "'" + sanitized;
        return $"\"{sanitized.Replace("\"", "\"\"")}\"";
    }
}
