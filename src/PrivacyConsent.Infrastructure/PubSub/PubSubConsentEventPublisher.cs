using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PrivacyConsent.Infrastructure.PubSub;

public class PubSubConsentEventPublisher : IConsentEventPublisher
{
    private readonly PublisherClient _publisher;
    private readonly ILogger<PubSubConsentEventPublisher> _logger;

    public PubSubConsentEventPublisher(PublisherClient publisher, ILogger<PubSubConsentEventPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishDecisionAsync(long decisionAuditId, int? ownerId = null)
    {
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(new { decision_audit_id = decisionAuditId }))
        };

        if (ownerId.HasValue)
            message.Attributes["owner_id"] = ownerId.Value.ToString();

        var messageId = await _publisher.PublishAsync(message);
        _logger.LogDebug("Published decision audit {AuditId} as message {MessageId}", decisionAuditId, messageId);
    }

    public async Task PublishDecisionsAsync(IEnumerable<long> decisionAuditIds, int? ownerId = null)
    {
        await Task.WhenAll(decisionAuditIds.Select(id => PublishDecisionAsync(id, ownerId)));
    }
}
