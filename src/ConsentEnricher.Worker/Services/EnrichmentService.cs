using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.DTOs.Common;
using PrivacyConsent.Domain.Models;
using Microsoft.Extensions.Options;

namespace ConsentEnricher.Worker.Services;

public class EnrichmentService
{
    private readonly EnricherQueries _queries;
    private readonly EnrichmentConfig _config;
    private readonly ILogger<EnrichmentService> _logger;

    public EnrichmentService(
        EnricherQueries queries,
        IOptions<EnrichmentConfig> config,
        ILogger<EnrichmentService> logger)
    {
        _queries = queries;
        _config = config.Value;
        _logger = logger;
    }

    public HashSet<string> DefineKeySet(int? ownerId)
    {
        var keySet = new HashSet<string>(_config.DefaultKeySet);
        if (ownerId.HasValue && _config.OwnerKeySetExtensions.TryGetValue(ownerId.Value, out var extensions))
        {
            keySet.UnionWith(extensions);
        }
        return keySet;
    }

    public EnrichedConsentDto? ProcessResults(List<EnricherQueries.ConsentRelationRow> queryResults)
    {
        if (queryResults.Count == 0)
            return null;

        var first = queryResults[0];

        return new EnrichedConsentDto
        {
            Uuid = Guid.NewGuid(), // UUID v1 in original, using v4 for simplicity
            UserId = first.UserId,
            IdTypeId = first.IdTypeId,
            Ids = queryResults.Select(r => new EnrichedIdDto
            {
                UserId = r.MasterUserId,
                IdTypeId = r.MasterIdTypeId
            }).ToList(),
            IsAgreed = first.IsAgreed,
            ConsentExpressionId = first.ConsentExpressionId,
            ConsentId = first.ConsentId,
            ConsentTypeId = first.ConsentTypeId,
            ChangeContext = first.ChangeContext,
            OwnerId = first.OwnerId,
            ProductId = first.ProductId,
            DecisionId = first.DecisionId,
            LastDecisionDate = first.LastDecisionDate,
            DecisionAuditDate = first.DecisionAuditDate,
            UserConsentSourceId = first.UserConsentSourceId,
            ConsentName = first.ConsentName,
            ConsentExpressionName = first.ConsentExpressionName,
            ConsentExpressionDescription = first.ConsentExpressionDescription,
            Title = first.Title,
            ShortText = first.ShortText,
            LongText = first.LongText,
            PresentedLanguage = first.PresentedLanguage
        };
    }

    public EnrichedConsentDto? FilterByKeySet(EnrichedConsentDto data, HashSet<string> keySet)
    {
        if (data == null) return null;

        // Only null out fields that are NOT in the key set
        // The key set determines which fields are included in the output
        if (!keySet.Contains("consent_name")) data.ConsentName = null;
        if (!keySet.Contains("consent_expression_name")) data.ConsentExpressionName = null;
        if (!keySet.Contains("consent_expression_description")) data.ConsentExpressionDescription = null;
        if (!keySet.Contains("title")) data.Title = null;
        if (!keySet.Contains("short_text")) data.ShortText = null;
        if (!keySet.Contains("long_text")) data.LongText = null;
        if (!keySet.Contains("presented_language")) data.PresentedLanguage = null;
        if (!keySet.Contains("uuid")) data.Uuid = Guid.Empty;

        return data;
    }

    public async Task<EnrichedConsentDto?> EnrichConsentValueAsync(long decisionAuditId)
    {
        var rows = await _queries.GetConsentRelations(decisionAuditId);
        var data = ProcessResults(rows);

        if (data == null)
        {
            _logger.LogWarning("No consent relations found for decision audit {AuditId}", decisionAuditId);
            return null;
        }

        var keySet = DefineKeySet(data.OwnerId);
        return FilterByKeySet(data, keySet);
    }
}
