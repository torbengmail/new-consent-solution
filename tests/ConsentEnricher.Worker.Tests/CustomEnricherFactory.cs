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
using PrivacyConsent.Infrastructure.PubSub;
using Testcontainers.PostgreSql;
using TestUtilities;

namespace ConsentEnricher.Worker.Tests;

/// <summary>
/// WebApplicationFactory for the Consent Enricher Worker with Testcontainers
/// PostgreSQL and GCP Pub/Sub emulator. Applies the same migrations and seed
/// data as the Clojure project. The enricher publishes enriched output to
/// the Pub/Sub emulator for test verification.
/// </summary>
public class CustomEnricherFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string ProjectId = "test-project";
    private const string EnrichedTopicId = "consent-decisions-enriched";
    private const string EnrichedSubscriptionId = "consent-decisions-enriched-test";

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

    private PublisherClient? _enrichedPublisher;
    private SubscriberServiceApiClient? _subscriberAdmin;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _pgContainer.StartAsync(),
            _pubsubContainer.StartAsync());

        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await ClojureProjectHelper.SetupTestDatabase(conn);

        await SetupPubSub();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_enrichedPublisher != null)
            await _enrichedPublisher.ShutdownAsync(TimeSpan.FromSeconds(5));

        await Task.WhenAll(
            _pgContainer.DisposeAsync().AsTask(),
            _pubsubContainer.DisposeAsync().AsTask());

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with test container
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PrivacyDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<PrivacyDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // Replace enriched publisher with emulator-backed real publisher
            var pubDescriptors = services.Where(d => d.ServiceType == typeof(IEnrichedEventPublisher)).ToList();
            foreach (var d in pubDescriptors)
                services.Remove(d);

            services.AddSingleton<IEnrichedEventPublisher>(
                new PubSubEnrichedEventPublisher(
                    _enrichedPublisher!,
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<PubSubEnrichedEventPublisher>.Instance));
        });
    }

    /// <summary>
    /// Pull messages from the enriched subscription for test verification.
    /// </summary>
    public async Task<List<ReceivedMessage>> PullEnrichedMessages(int maxMessages = 10)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, EnrichedSubscriptionId);
        var response = await _subscriberAdmin!.PullAsync(subscriptionName, maxMessages);
        return response.ReceivedMessages.ToList();
    }

    public async Task AcknowledgeMessages(IEnumerable<ReceivedMessage> messages)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, EnrichedSubscriptionId);
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

        var topicName = TopicName.FromProjectTopic(ProjectId, EnrichedTopicId);
        await publisherAdmin.CreateTopicAsync(topicName);

        _subscriberAdmin = await new SubscriberServiceApiClientBuilder
        {
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync();

        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, EnrichedSubscriptionId);
        await _subscriberAdmin.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);

        _enrichedPublisher = await new PublisherClientBuilder
        {
            TopicName = topicName,
            EmulatorDetection = EmulatorDetection.EmulatorOnly,
        }.BuildAsync();
    }
}
