namespace PrivacyConsent.Infrastructure.PubSub;

public interface IConsentEventPublisher
{
    Task PublishDecisionAsync(long decisionAuditId, int? ownerId = null);
    Task PublishDecisionsAsync(IEnumerable<long> decisionAuditIds, int? ownerId = null);
}
