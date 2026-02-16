using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PrivacyConsent.Data;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Infrastructure.Cache;
using PrivacyConsent.Infrastructure.Email;
using PrivacyConsent.Infrastructure.ExternalApis;
using PrivacyConsent.Infrastructure.PubSub;
using PrivacyConsent.Infrastructure.Storage;
using PrivacyService.Api.Auth;
using PrivacyService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("PrivacyDb"));
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<PrivacyDbContext>(options =>
    options.UseNpgsql(dataSource));

// Memory cache for RBAC TTL
builder.Services.AddMemoryCache();

// Authentication
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", null);
builder.Services.AddAuthorization();

// Query services
builder.Services.AddScoped<IConsentQueries, ConsentQueries>();
builder.Services.AddScoped<IUserConsentQueries, UserConsentQueries>();
builder.Services.AddScoped<IExpressionQueries, ExpressionQueries>();
builder.Services.AddScoped<IRbacQueries, RbacQueries>();
builder.Services.AddScoped<ICacheQueries, CacheQueries>();
builder.Services.AddScoped<IDsrQueries, DsrQueries>();
builder.Services.AddScoped<IDictionaryQueries, DictionaryQueries>();
builder.Services.AddScoped<ITranslationQueries, TranslationQueries>();
builder.Services.AddScoped<IRequestAttemptQueries, RequestAttemptQueries>();
builder.Services.AddScoped<IDataDumpQueries, DataDumpQueries>();
builder.Services.AddScoped<IMasterIdQueries, MasterIdQueries>();
builder.Services.AddScoped<IAdminConsentQueries, AdminConsentQueries>();
builder.Services.AddScoped<IUserManagementQueries, UserManagementQueries>();
builder.Services.AddScoped<IUserConsentSourceQueries, UserConsentSourceQueries>();

// Infrastructure services
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<UserDataCacheService>();
builder.Services.AddHttpClient<IDenmarkApiClient, DenmarkApiClient>();
builder.Services.AddHttpClient<IZendeskClient, ZendeskClient>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IFileStorageService, GcsFileStorageService>();
builder.Services.AddScoped<IConsentEventPublisher, PubSubConsentEventPublisher>();

// Application services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDecisionService, DecisionService>();
builder.Services.AddScoped<IDataSubjectRightsService, DataSubjectRightsService>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
        context =>
        {
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(remoteIp,
                _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100),
                    Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60)),
                    QueueLimit = 0
                });
        });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<PrivacyService.Api.Middleware.CorrelationIdMiddleware>();
app.UseMiddleware<PrivacyService.Api.Middleware.GlobalExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/ping", () => "pong");

app.Run();

// Make Program class accessible for WebApplicationFactory in tests
public partial class Program { }
