using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PrivacyConsent.Data;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Infrastructure.Email;
using PrivacyConsent.Infrastructure.ExternalApis;
using PrivacyConsent.Infrastructure.PubSub;
using PrivacyConsent.Infrastructure.Storage;
using Testcontainers.PostgreSql;
using TestUtilities;

namespace PrivacyService.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory with Testcontainers PostgreSQL and GCP Pub/Sub emulator.
/// Applies Clojure project SQL migrations, seeds test data, and creates Pub/Sub topics.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ProjectId = "test-project";
    private const string RawTopicId = "consent-decisions-raw";
    private const string RawSubscriptionId = "consent-decisions-raw-test";

    private readonly PostgreSqlContainer _pgContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("privacy")
        .WithUsername("privacy")
        .WithPassword("privacy")
        .Build();

    private readonly IContainer _pubsubContainer = new ContainerBuilder()
        .WithImage("gcr.io/google.com/cloudsdktool/google-cloud-cli:emulators")
        .WithCommand("gcloud", "beta", "emulators", "pubsub", "start",
            "--host-port=0.0.0.0:8085", $"--project={ProjectId}")
        .WithPortBinding(8085, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Server started"))
        .Build();

    public string ConnectionString => _pgContainer.GetConnectionString();
    public string PubSubEmulatorHost => $"{_pubsubContainer.Hostname}:{_pubsubContainer.GetMappedPublicPort(8085)}";

    private PublisherClient? _rawPublisher;
    private SubscriberServiceApiClient? _subscriberAdmin;

    public async Task InitializeAsync()
    {
        // Start both containers in parallel
        await Task.WhenAll(
            _pgContainer.StartAsync(),
            _pubsubContainer.StartAsync());

        // Set up PostgreSQL: schemas, migrations, seed data
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();

        var supplementPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "seed-supplement.sql");
        supplementPath = Path.GetFullPath(supplementPath);

        await ClojureProjectHelper.SetupTestDatabase(conn, supplementPath);

        // Set up Pub/Sub emulator: topics, subscriptions, publisher
        await SetupPubSub();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_rawPublisher != null)
            await _rawPublisher.ShutdownAsync(TimeSpan.FromSeconds(5));

        await Task.WhenAll(
            _pgContainer.DisposeAsync().AsTask(),
            _pubsubContainer.DisposeAsync().AsTask());

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RateLimiting:PermitLimit", "10000");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with test container
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PrivacyDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<PrivacyDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // Replace Pub/Sub publisher with emulator-backed real publisher
            ReplaceService<IConsentEventPublisher>(services,
                new PubSubConsentEventPublisher(
                    _rawPublisher!,
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<PubSubConsentEventPublisher>.Instance));

            // Replace remaining infrastructure services with mocks
            ReplaceService<IDenmarkApiClient>(services, new MockDenmarkApiClient());
            ReplaceService<IZendeskClient>(services, new MockZendeskClient());
            ReplaceService<IEmailService>(services, new MockEmailService());
            ReplaceService<IFileStorageService>(services, new MockFileStorageService());
        });
    }

    /// <summary>
    /// Pull messages from the raw decisions subscription for test verification.
    /// </summary>
    public async Task<List<ReceivedMessage>> PullRawMessages(int maxMessages = 10)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, RawSubscriptionId);
        var response = await _subscriberAdmin!.PullAsync(subscriptionName, maxMessages);
        return response.ReceivedMessages.ToList();
    }

    /// <summary>
    /// Acknowledge messages after pulling them.
    /// </summary>
    public async Task AcknowledgeMessages(IEnumerable<ReceivedMessage> messages)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, RawSubscriptionId);
        await _subscriberAdmin!.AcknowledgeAsync(subscriptionName,
            messages.Select(m => m.AckId));
    }

    private async Task SetupPubSub()
    {
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", PubSubEmulatorHost);

        var publisherAdmin = await new PublisherServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync();

        var topicName = TopicName.FromProjectTopic(ProjectId, RawTopicId);
        await publisherAdmin.CreateTopicAsync(topicName);

        _subscriberAdmin = await new SubscriberServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync();

        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, RawSubscriptionId);
        await _subscriberAdmin.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);

        _rawPublisher = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync();
    }

    private static void ReplaceService<T>(IServiceCollection services, T implementation) where T : class
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors)
            services.Remove(d);

        services.AddSingleton<T>(implementation);
    }

    // ----- Mock implementations for services not under test -----

    private class MockDenmarkApiClient : IDenmarkApiClient
    {
        public Task<DenmarkUserInfo?> GetUserInfoAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult<DenmarkUserInfo?>(new DenmarkUserInfo { IsUser = true });

        public Task<bool> IsUserAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult(true);

        public Task<bool> IsCbbUserAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult(false);

        public Task<bool> IsNemIdValidatedAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult(false);

        public Task<List<DenmarkDsrRequest>> GetUserRequestsAsync(string userId, int idTypeId, string connectIdToken, string requestType) =>
            Task.FromResult(new List<DenmarkDsrRequest>());

        public Task<string?> CreateUserRequestAsync(int idTypeId, string connectIdToken, DenmarkCreateRequestParams request) =>
            Task.FromResult<string?>("mock-ticket-id");

        public Task<Dictionary<string, object>> GetPendingRequestsAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult(new Dictionary<string, object>());

        public Task<List<DenmarkFinishedRequest>> GetFinishedRequestsAsync(string userId, int idTypeId, string connectIdToken) =>
            Task.FromResult(new List<DenmarkFinishedRequest>());

        public DenmarkFileSharingResult GetFileSharingLinks(List<DenmarkFinishedRequest> data, string userId, int idTypeId, string connectIdToken, string language) =>
            new() { Links = [] };
    }

    private class MockZendeskClient : IZendeskClient
    {
        public Task<string?> CreateTicketAsync(string subject, string body, string requesterEmail) =>
            Task.FromResult<string?>("mock-zendesk-ticket");

        public Task<Dictionary<string, object>> GetRequestStatusesAsync(string email, string ownerTag) =>
            Task.FromResult(new Dictionary<string, object>());

        public Task<List<ZendeskFileInfo>> GetPersonalDataFilesAsync(string userId) =>
            Task.FromResult(new List<ZendeskFileInfo>());

        public ZendeskFileSharingResult GetFileSharingLinks(List<ZendeskFileInfo> data) =>
            new() { Links = [] };
    }

    private class MockEmailService : IEmailService
    {
        public Task<string?> SendEmailAsync(string from, string to, string subject, string body) =>
            Task.FromResult<string?>("mock-message-id");

        public Task SendDsrNotificationEmailAsync(string userId, string email, string right, string? note) =>
            Task.CompletedTask;
    }

    private class MockFileStorageService : IFileStorageService
    {
        public Task<string> UploadFileAsync(string bucketName, string key, Stream content, string contentType) =>
            Task.FromResult($"mock://{bucketName}/{key}");

        public Task<string> GenerateUploadLinkAsync(string bucketName, string key, TimeSpan expiration) =>
            Task.FromResult($"https://mock-storage.example.com/{bucketName}/{key}?expires=3600");

        public Task<Stream?> DownloadFileAsync(string bucketName, string key) =>
            Task.FromResult<Stream?>(new MemoryStream());
    }
}

/// <summary>
/// xUnit collection definition for sharing the CustomWebApplicationFactory across test classes.
/// Apply [Collection("Api")] to test classes that need the shared factory.
/// </summary>
[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
