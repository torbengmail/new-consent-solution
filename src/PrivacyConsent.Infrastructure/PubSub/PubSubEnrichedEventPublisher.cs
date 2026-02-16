using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Infrastructure.PubSub;

public class PubSubEnrichedEventPublisher : IEnrichedEventPublisher
{
    private readonly PublisherClient _publisher;
    private readonly ILogger<PubSubEnrichedEventPublisher> _logger;

    public PubSubEnrichedEventPublisher(PublisherClient publisher, ILogger<PubSubEnrichedEventPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublishEnrichedAsync(EnrichedConsentDto data)
    {
        var message = new PubsubMessage
        {
            Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            }))
        };

        if (data.OwnerId.HasValue)
            message.Attributes["owner_id"] = data.OwnerId.Value.ToString();

        var messageId = await _publisher.PublishAsync(message);
        _logger.LogDebug("Published enriched consent for decision {DecisionId} as message {MessageId}",
            data.DecisionId, messageId);
    }
}
