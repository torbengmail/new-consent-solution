using ConsentEnricher.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.Models;
using TestUtilities;

namespace ConsentEnricher.Worker.Tests;

public class EnrichmentServiceTests
{
    private static EnrichmentService CreateService()
    {
        var config = new EnrichmentConfig
        {
            DefaultKeySet =
            [
                "uuid",
                "user_id",
                "id_type_id",
                "ids",
                "is_agreed",
                "consent_expression_id",
                "consent_id",
                "consent_type_id",
                "change_context",
                "owner_id",
                "product_id",
                "decision_id",
                "last_decision_date",
                "decision_audit_date",
                "user_consent_source_id"
            ],
            OwnerKeySetExtensions = new Dictionary<int, HashSet<string>>
            {
                [OwnerConstants.DenmarkOwnerId] =
                [
                    "consent_name",
                    "consent_expression_name",
                    "consent_expression_description",
                    "title",
                    "short_text",
                    "long_text",
                    "presented_language"
                ]
            }
        };

        var options = Options.Create(config);
        return new EnrichmentService(null!, options, NullLogger<EnrichmentService>.Instance);
    }

    [Fact]
    public void DefineKeySet_UnknownOwner_ReturnsDefaultKeysOnly()
    {
        var service = CreateService();

        var keys = service.DefineKeySet(ownerId: 999);

        Assert.Equal(15, keys.Count);
        Assert.Contains("uuid", keys);
        Assert.Contains("consent_id", keys);
        Assert.DoesNotContain("consent_name", keys);
        Assert.DoesNotContain("title", keys);
    }

    [Fact]
    public void DefineKeySet_DenmarkOwner_ReturnsExtendedKeys()
    {
        var service = CreateService();

        var keys = service.DefineKeySet(ownerId: OwnerConstants.DenmarkOwnerId);

        Assert.Equal(22, keys.Count);
        Assert.Contains("uuid", keys);
        Assert.Contains("consent_name", keys);
        Assert.Contains("title", keys);
        Assert.Contains("short_text", keys);
        Assert.Contains("long_text", keys);
        Assert.Contains("presented_language", keys);
    }

    [Fact]
    public void ProcessResults_EmptyInput_ReturnsNull()
    {
        var service = CreateService();

        var result = service.ProcessResults([]);

        Assert.Null(result);
    }

    [Fact]
    public void ProcessResults_SingleRow_MapsAllFieldsCorrectly()
    {
        var service = CreateService();

        var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var rows = new List<EnricherQueries.ConsentRelationRow>
        {
            new()
            {
                ConsentExpressionId = 101,
                ConsentExpressionName = "test-expression",
                ConsentExpressionDescription = "test-description",
                ConsentId = 42,
                ConsentName = "test-consent",
                ConsentTypeId = 3,
                OwnerId = SeedData.DenmarkOwnerId,
                ProductId = SeedData.CaptureProductId,
                DecisionId = SeedData.DecisionId7000,
                DecisionAuditDate = testDate,
                LastDecisionDate = testDate,
                UserId = "user-123",
                IdTypeId = SeedData.ConnectIdType,
                ChangeContext = "{}",
                UserConsentSourceId = SeedData.UserConsentSourceId,
                IsAgreed = true,
                PresentedLanguage = "en",
                Title = "Stay updated",
                ShortText = "SHORT TEXT",
                LongText = "LONG LEGAL TEXT",
                MasterUserId = "master-123",
                MasterIdTypeId = SeedData.ConnectIdType
            }
        };

        var result = service.ProcessResults(rows);

        Assert.NotNull(result);
        Assert.Equal("user-123", result.UserId);
        Assert.Equal(SeedData.ConnectIdType, result.IdTypeId);
        Assert.True(result.IsAgreed, "Expected is_agreed=true");
        Assert.Equal(42, result.ConsentId);
        Assert.Equal(3, result.ConsentTypeId);
        Assert.Equal(SeedData.DenmarkOwnerId, result.OwnerId);
        Assert.Equal(SeedData.CaptureProductId, result.ProductId);
        Assert.Equal(SeedData.DecisionId7000, result.DecisionId);
        Assert.Equal(testDate, result.DecisionAuditDate);
        Assert.Equal("test-consent", result.ConsentName);
        Assert.Equal("test-expression", result.ConsentExpressionName);
        Assert.Equal("en", result.PresentedLanguage);
        Assert.Equal("Stay updated", result.Title);
        Assert.Single(result.Ids);
        Assert.Equal("master-123", result.Ids[0].UserId);
    }
}
