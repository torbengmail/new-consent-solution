using PrivacyConsent.Data.Queries;
using PrivacyConsent.Infrastructure.PubSub;

namespace PrivacyService.Api.Services;

public class DecisionService : IDecisionService
{
    private readonly IUserConsentQueries _userConsentQueries;
    private readonly IMasterIdQueries _masterIdQueries;
    private readonly IConsentQueries _consentQueries;
    private readonly IConsentEventPublisher _eventPublisher;
    private readonly ILogger<DecisionService> _logger;

    public DecisionService(
        IUserConsentQueries userConsentQueries,
        IMasterIdQueries masterIdQueries,
        IConsentQueries consentQueries,
        IConsentEventPublisher eventPublisher,
        ILogger<DecisionService> logger)
    {
        _userConsentQueries = userConsentQueries;
        _masterIdQueries = masterIdQueries;
        _consentQueries = consentQueries;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<List<long>> SaveDecisions(
        List<DecisionInput> decisions,
        string userId, int idTypeId)
    {
        var master = await _masterIdQueries.GetOrCreateMasterId(userId, idTypeId);
        if (master == null)
            throw new InvalidOperationException($"Failed to get/create master ID for user {userId}");

        // Batch-fetch all consent info in a single query (fixes N+1)
        var consentInfoMap = await _consentQueries.GetConsentInfoByExpressions(
            decisions.Select(d => d.ConsentExpressionId).Distinct());

        var auditIds = new List<long>();

        foreach (var decision in decisions)
        {
            if (!consentInfoMap.TryGetValue(decision.ConsentExpressionId, out var consentInfo))
            {
                _logger.LogWarning("Consent not found for expression {ExpressionId}", decision.ConsentExpressionId);
                continue;
            }

            var decisionId = await _userConsentQueries.UpsertUserConsent(
                master.Id,
                consentInfo.ConsentId,
                decision.ConsentExpressionId,
                decision.ParentConsentExpressionId,
                decision.IsAgreed,
                decision.UserConsentSourceId,
                decision.PresentedLanguage,
                decision.ChangeContext,
                idTypeId,
                consentInfo.OwnerId,
                userId);

            var auditId = await _userConsentQueries.CreateAuditTrail(
                decisionId,
                decision.ConsentExpressionId,
                decision.ParentConsentExpressionId,
                decision.IsAgreed,
                decision.PresentedLanguage,
                decision.UserConsentSourceId,
                decision.ChangeContext,
                userId,
                idTypeId);

            auditIds.Add(auditId);

            // Publish to Pub/Sub
            await _eventPublisher.PublishDecisionAsync(auditId, consentInfo.OwnerId);
        }

        return auditIds;
    }

}

public class DecisionInput
{
    public int ConsentExpressionId { get; set; }
    public int? ParentConsentExpressionId { get; set; }
    public bool IsAgreed { get; set; }
    public int UserConsentSourceId { get; set; }
    public string PresentedLanguage { get; set; } = "en";
    public string? ChangeContext { get; set; }
}
