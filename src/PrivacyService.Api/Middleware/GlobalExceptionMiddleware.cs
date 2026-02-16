using System.Text.Json;
using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyService.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            var errorId = Guid.NewGuid();

            _logger.LogError(ex, "Unhandled exception. ErrorId: {ErrorId}, CorrelationId: {CorrelationId}", errorId, correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                ErrorId = errorId,
                ErrorCode = "INTERNAL_ERROR",
                ErrorMessage = _env.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred"
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            await context.Response.WriteAsync(json);
        }
    }
}
