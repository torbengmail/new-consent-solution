using PrivacyConsent.Domain.DTOs.Common;

namespace PrivacyConsent.Infrastructure.PubSub;

public interface IEnrichedEventPublisher
{
    Task PublishEnrichedAsync(EnrichedConsentDto data);
}
