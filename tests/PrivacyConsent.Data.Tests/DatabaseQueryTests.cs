using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data;
using PrivacyConsent.Data.Queries;
using TestUtilities;

namespace PrivacyConsent.Data.Tests;

[Collection("Database")]
public class DatabaseQueryTests
{
    private readonly TestDatabaseFixture _fixture;

    public DatabaseQueryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private PrivacyDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PrivacyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        return new PrivacyDbContext(options);
    }

    // ── ConsentQueries ──────────────────────────────────────────────

    [Fact]
    public async Task GetConsents_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new ConsentQueries(db);

        var consents = await queries.GetConsents();

        Assert.NotEmpty(consents);
    }

    [Fact]
    public async Task GetConsentsByUseCase_ExistingUseCase_ReturnsExpectedConsents()
    {
        await using var db = CreateDbContext();
        var queries = new ConsentQueries(db);

        var consents = await queries.GetConsentsByUseCase(
            ownerId: SeedData.DefaultOwnerId, useCaseId: SeedData.UseCaseId);

        Assert.NotEmpty(consents);
        var ids = consents.Select(c => c.ConsentId).OrderBy(x => x).ToList();
        Assert.Contains(SeedData.ConsentId, ids);
        Assert.Contains(203, ids);
    }

    [Fact]
    public async Task GetConsentById_ExistingConsent_ReturnsConsentWithName()
    {
        await using var db = CreateDbContext();
        var queries = new ConsentQueries(db);

        var consent = await queries.GetConsentById(SeedData.ConsentId);

        Assert.NotNull(consent);
        Assert.Equal(SeedData.ConsentId, consent.ConsentId);
        Assert.Equal("direct marketing by sms", consent.Name);
    }

    [Fact]
    public async Task GetConsentInfoByExpression_ExistingExpression_ReturnsConsentIdAndOwner()
    {
        await using var db = CreateDbContext();
        var queries = new ConsentQueries(db);

        var info = await queries.GetConsentInfoByExpression(SeedData.ExpressionId);

        Assert.NotNull(info);
        Assert.Equal(SeedData.ConsentId, info.Value.ConsentId);
        Assert.Equal(SeedData.DefaultOwnerId, info.Value.OwnerId);
    }

    // ── UserConsentQueries ──────────────────────────────────────────

    [Fact]
    public async Task GetUserConsentDecisionsBatch_DenmarkOwner_ReturnsDecisionsForOwner()
    {
        await using var db = CreateDbContext();
        var queries = new UserConsentQueries(db);

        var results = await queries.GetUserConsentDecisionsBatch(
            ownerId: SeedData.DenmarkOwnerId, consentId: null, offset: 0, limit: 100);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(SeedData.DenmarkOwnerId, r.OwnerId));
    }

    [Fact]
    public async Task UpsertUserConsent_ExistingDecision_UpdatesAndRestores()
    {
        await using var db = CreateDbContext();
        var ucQueries = new UserConsentQueries(db);
        var masterQueries = new MasterIdQueries(db);

        var master = await masterQueries.GetMasterId(SeedData.TestUserId, SeedData.ConnectIdType);
        Assert.NotNull(master);

        // Update to is_agreed=false, then restore
        try
        {
            var id = await ucQueries.UpsertUserConsent(
                master.Id, consentId: SeedData.ConsentExpressionId111,
                consentExpressionId: SeedData.ConsentExpressionId111,
                parentConsentExpressionId: null, isAgreed: false,
                userConsentSourceId: SeedData.UserConsentSourceId,
                presentedLanguage: "en", changeContext: "{}",
                idTypeId: SeedData.ConnectIdType, ownerId: SeedData.DenmarkOwnerId);

            Assert.Equal(SeedData.DecisionId7000, id);
        }
        finally
        {
            // Restore to original state (is_agreed=true)
            await ucQueries.UpsertUserConsent(
                master.Id, consentId: SeedData.ConsentExpressionId111,
                consentExpressionId: SeedData.ConsentExpressionId111,
                parentConsentExpressionId: null, isAgreed: true,
                userConsentSourceId: SeedData.UserConsentSourceId,
                presentedLanguage: "en", changeContext: "{}",
                idTypeId: SeedData.ConnectIdType, ownerId: SeedData.DenmarkOwnerId);
        }
    }

    [Fact]
    public async Task CreateAuditTrail_ValidParams_ReturnsPositiveId()
    {
        await using var db = CreateDbContext();
        var queries = new UserConsentQueries(db);

        var auditId = await queries.CreateAuditTrail(
            decisionId: SeedData.DecisionId7000,
            consentExpressionId: SeedData.ConsentExpressionId111,
            parentConsentExpressionId: null, isAgreed: true,
            presentedLanguage: "en",
            userConsentSourceId: SeedData.UserConsentSourceId,
            changeContext: "{}", userId: SeedData.TestUserId,
            idTypeId: SeedData.ConnectIdType);

        Assert.True(auditId > 0, "Audit trail ID should be positive");
    }

    [Fact]
    public async Task ReadDecisionHistory_ExistingUser_ReturnsHistoryInDescOrder()
    {
        await using var db = CreateDbContext();
        var queries = new UserConsentQueries(db);

        var history = await queries.ReadDecisionHistory(
            SeedData.TestUserId, SeedData.ConnectIdType, SeedData.ConsentExpressionId111);

        Assert.NotEmpty(history);
        if (history.Count > 1)
            Assert.True(history[0].Date >= history[1].Date, "History should be ordered by date desc");
    }

    [Fact]
    public async Task GetUserConsentDecisionsShort_ExistingDecision_ReturnsAgreed()
    {
        await using var db = CreateDbContext();
        var queries = new UserConsentQueries(db);

        var requests = new List<(int, string, int)>
        {
            (SeedData.ConsentExpressionId111, SeedData.TestUserId2, SeedData.ConnectIdType)
        };
        var results = await queries.GetUserConsentDecisionsShort(requests);

        Assert.NotEmpty(results);
        var row = results.First();
        Assert.Equal(SeedData.ConsentExpressionId111, row.ConsentId);
        Assert.Equal(SeedData.TestUserId2, row.UserId);
        Assert.True(row.IsAgreed, "Expected is_agreed=true for test user 2222");
    }

    // ── MasterIdQueries ─────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateMasterId_ExistingUser_ReturnsKnownGuid()
    {
        await using var db = CreateDbContext();
        var queries = new MasterIdQueries(db);

        var master = await queries.GetOrCreateMasterId(SeedData.TestUserId, SeedData.ConnectIdType);

        Assert.NotNull(master);
        Assert.Equal(SeedData.TestUserId, master.UserId);
        Assert.Equal(Guid.Parse(SeedData.MasterGuid222), master.Id);
    }

    [Fact]
    public async Task GetOrCreateMasterId_NewUser_CreatesNewGuid()
    {
        await using var db = CreateDbContext();
        var queries = new MasterIdQueries(db);

        var master = await queries.GetOrCreateMasterId("newuser999", SeedData.ConnectIdType);

        Assert.NotNull(master);
        Assert.Equal("newuser999", master.UserId);
        Assert.NotEqual(Guid.Empty, master.Id);
    }

    [Fact]
    public async Task GetMasterId_NonExistentUser_ReturnsNull()
    {
        await using var db = CreateDbContext();
        var queries = new MasterIdQueries(db);

        var master = await queries.GetMasterId("nonexistent", SeedData.ConnectIdType);

        Assert.Null(master);
    }

    // ── DictionaryQueries ───────────────────────────────────────────

    [Fact]
    public async Task GetConsentTypes_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var types = await queries.GetConsentTypes();

        Assert.NotEmpty(types);
    }

    [Fact]
    public async Task GetLanguages_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var languages = await queries.GetLanguages();

        Assert.NotEmpty(languages);
    }

    [Fact]
    public async Task GetOwners_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var owners = await queries.GetOwners();

        Assert.NotEmpty(owners);
    }

    [Fact]
    public async Task GetConsentPurposes_Seeded_ReturnsAtLeastSix()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var purposes = await queries.GetPurposeCategories();

        Assert.NotEmpty(purposes);
        Assert.True(purposes.Count >= 6, "Expected at least 6 purpose categories from seed data");
    }

    [Fact]
    public async Task GetExpressionStatuses_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var statuses = await queries.GetExpressionStatuses();

        Assert.NotEmpty(statuses);
    }

    [Fact]
    public async Task GetExpressionTags_Seeded_ContainsExpectedTags()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var tags = await queries.GetExpressionTags();

        Assert.NotEmpty(tags);
        Assert.Contains(tags, t => t.Name == "privacy-dashboard");
        Assert.Contains(tags, t => t.Name == "capture");
    }

    [Fact]
    public async Task GetIdTypes_Seeded_ReturnsNonEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DictionaryQueries(db);

        var idTypes = await queries.GetIdTypes();

        Assert.NotEmpty(idTypes);
    }

    // ── ExpressionQueries ───────────────────────────────────────────

    [Fact]
    public async Task GetRandExpressionsByProductId_ExistingData_ReturnsExpressions()
    {
        await using var db = CreateDbContext();
        var queries = new ExpressionQueries(db);

        var results = await queries.GetRandExpressionsByProductId(
            ownerId: SeedData.DefaultOwnerId, productId: SeedData.CaptureProductId,
            userId: SeedData.TestUserId, idTypeId: SeedData.ConnectIdType,
            language: "en", tag: null);

        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task GetExpressionsByConsentId_ExistingConsent_ContainsExpectedExpression()
    {
        await using var db = CreateDbContext();
        var queries = new ExpressionQueries(db);

        var results = await queries.GetExpressionsByConsentId(
            consentId: SeedData.ConsentId, userId: SeedData.TestUserId,
            idTypeId: SeedData.ConnectIdType, language: "en", tag: null);

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.ConsentExpressionId == SeedData.ExpressionId);
    }

    // ── RequestAttemptQueries ───────────────────────────────────────

    [Fact]
    public async Task RequestAttempts_SeededUser_HasExpectedCount()
    {
        await using var db = CreateDbContext();
        var masterQueries = new MasterIdQueries(db);

        var master = await masterQueries.GetMasterId(SeedData.TestUserId, SeedData.ConnectIdType);
        Assert.NotNull(master);

        var attempt = await db.RequestAttempts
            .FirstOrDefaultAsync(ra =>
                ra.MasterId == master.Id &&
                ra.ConsentId == SeedData.ConsentExpressionId111 &&
                ra.ConsentExpressionId == SeedData.ConsentExpressionId111);

        Assert.NotNull(attempt);
        Assert.Equal(42, attempt.AttemptsCount);
    }

    // ── DataDumpQueries ─────────────────────────────────────────────

    [Fact]
    public async Task GetUserDataDump_ExistingUser_ReturnsDecisionsAndAttempts()
    {
        await using var db = CreateDbContext();
        var queries = new DataDumpQueries(db);

        var decisions = await queries.GetUserDecisionDataRecords(SeedData.TestUserId, SeedData.ConnectIdType);
        var attempts = await queries.GetUserRequestAttemptDataRecords(SeedData.TestUserId, SeedData.ConnectIdType);

        Assert.NotEmpty(decisions);
        Assert.NotEmpty(attempts);
    }

    [Fact]
    public async Task GetUserDataDump_NonExistentUser_ReturnsEmpty()
    {
        await using var db = CreateDbContext();
        var queries = new DataDumpQueries(db);

        var decisions = await queries.GetUserDecisionDataRecords("999", SeedData.ConnectIdType);
        var attempts = await queries.GetUserRequestAttemptDataRecords("999", SeedData.ConnectIdType);

        Assert.Empty(decisions);
        Assert.Empty(attempts);
    }

    // ── TranslationQueries ──────────────────────────────────────────

    [Fact]
    public async Task GetTranslations_ExistingOwner_ReturnsTranslationsWithEnglish()
    {
        await using var db = CreateDbContext();
        var queries = new TranslationQueries(db);

        var result = await queries.GetTranslations(SeedData.DefaultOwnerId, productId: null);

        Assert.NotEmpty(result);
        Assert.Contains(result, t => t.LangCode == "en");
    }
}
