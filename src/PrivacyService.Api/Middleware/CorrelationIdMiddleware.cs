namespace PrivacyService.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path.ToString()
        }))
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Request started: {Method} {Path}", context.Request.Method, context.Request.Path);

            await _next(context);

            sw.Stop();
            _logger.LogInformation("Request completed: {Method} {Path} {StatusCode} in {ElapsedMs}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
