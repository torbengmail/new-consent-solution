using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PrivacyConsent.Data.Entities;
using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyConsent.Infrastructure.Cache;
using PrivacyConsent.Infrastructure.Email;
using PrivacyConsent.Infrastructure.ExternalApis;
using PrivacyConsent.Infrastructure.PubSub;
using PrivacyService.Api.Services;

namespace PrivacyService.Api.Tests;

public class DecisionServiceTests
{
    private readonly IUserConsentQueries _userConsentQueries = Substitute.For<IUserConsentQueries>();
    private readonly IMasterIdQueries _masterIdQueries = Substitute.For<IMasterIdQueries>();
    private readonly IConsentQueries _consentQueries = Substitute.For<IConsentQueries>();
    private readonly IConsentEventPublisher _eventPublisher = Substitute.For<IConsentEventPublisher>();
    private readonly DecisionService _sut;

    public DecisionServiceTests()
    {
        _sut = new DecisionService(
            _userConsentQueries, _masterIdQueries, _consentQueries,
            _eventPublisher, NullLogger<DecisionService>.Instance);
    }

    [Fact]
    public async Task SaveDecisions_ReturnsAuditIds()
    {
        var masterId = new MasterId { Id = Guid.NewGuid() };
        _masterIdQueries.GetOrCreateMasterId("user1", 1).Returns(masterId);
        _consentQueries.GetConsentInfoByExpressions(Arg.Any<IEnumerable<int>>())
            .Returns(new Dictionary<int, (int, int?)> { [301] = (201, 1) });
        _userConsentQueries.UpsertUserConsent(
                masterId.Id, 201, 301, null, true, 1, "en", null, 1, 1, "user1")
            .Returns(10);
        _userConsentQueries.CreateAuditTrail(10, 301, null, true, "en", 1, null, "user1", 1)
            .Returns(9001L);

        var decisions = new List<DecisionInput>
        {
            new() { ConsentExpressionId = 301, IsAgreed = true, UserConsentSourceId = 1 }
        };

        var result = await _sut.SaveDecisions(decisions, "user1", 1);

        Assert.Single(result);
        Assert.Equal(9001L, result[0]);
        await _eventPublisher.Received(1).PublishDecisionAsync(9001L, 1);
    }

    [Fact]
    public async Task SaveDecisions_ThrowsWhenMasterIdNull()
    {
        _masterIdQueries.GetOrCreateMasterId("user1", 1).Returns((MasterId?)null);

        var decisions = new List<DecisionInput>
        {
            new() { ConsentExpressionId = 301, IsAgreed = true }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SaveDecisions(decisions, "user1", 1));
    }

    [Fact]
    public async Task SaveDecisions_SkipsUnknownExpression()
    {
        var masterId = new MasterId { Id = Guid.NewGuid() };
        _masterIdQueries.GetOrCreateMasterId("user1", 1).Returns(masterId);
        _consentQueries.GetConsentInfoByExpressions(Arg.Any<IEnumerable<int>>())
            .Returns(new Dictionary<int, (int, int?)>()); // empty = no consent found

        var decisions = new List<DecisionInput>
        {
            new() { ConsentExpressionId = 999, IsAgreed = true }
        };

        var result = await _sut.SaveDecisions(decisions, "user1", 1);

        Assert.Empty(result);
        await _userConsentQueries.DidNotReceive().UpsertUserConsent(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(),
            Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
            Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SaveDecisions_MultipleDecisions_ReturnAllAuditIds()
    {
        var masterId = new MasterId { Id = Guid.NewGuid() };
        _masterIdQueries.GetOrCreateMasterId("user1", 1).Returns(masterId);
        _consentQueries.GetConsentInfoByExpressions(Arg.Any<IEnumerable<int>>())
            .Returns(new Dictionary<int, (int, int?)>
            {
                [301] = (201, 1),
                [302] = (202, 1)
            });
        _userConsentQueries.UpsertUserConsent(
                masterId.Id, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(),
                Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>())
            .Returns(10, 11);
        _userConsentQueries.CreateAuditTrail(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<bool>(),
                Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<int?>())
            .Returns(9001L, 9002L);

        var decisions = new List<DecisionInput>
        {
            new() { ConsentExpressionId = 301, IsAgreed = true, UserConsentSourceId = 1 },
            new() { ConsentExpressionId = 302, IsAgreed = false, UserConsentSourceId = 1 }
        };

        var result = await _sut.SaveDecisions(decisions, "user1", 1);

        Assert.Equal(2, result.Count);
        Assert.Equal(9001L, result[0]);
        Assert.Equal(9002L, result[1]);
        await _eventPublisher.Received(2).PublishDecisionAsync(Arg.Any<long>(), Arg.Any<int?>());
    }
}

public class DashboardServiceTests
{
    private readonly IExpressionQueries _expressionQueries = Substitute.For<IExpressionQueries>();
    private readonly IDenmarkApiClient _denmarkApi = Substitute.For<IDenmarkApiClient>();
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _sut = new DashboardService(
            _expressionQueries, _denmarkApi, NullLogger<DashboardService>.Instance);
    }

    [Fact]
    public async Task AdjustOwnerIds_NoToken_ReturnsOriginalList()
    {
        var result = await _sut.AdjustOwnerIds([1, 2], "user1", 1, null);

        Assert.Equal([1, 2], result);
        await _denmarkApi.DidNotReceive().IsUserAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task AdjustOwnerIds_DenmarkUser_AddsDenmarkOwner()
    {
        _denmarkApi.IsUserAsync("user1", 1, "token").Returns(true);
        _denmarkApi.IsCbbUserAsync("user1", 1, "token").Returns(false);

        var result = await _sut.AdjustOwnerIds([1], "user1", 1, "token");

        Assert.Contains(OwnerConstants.DenmarkOwnerId, result);
        Assert.DoesNotContain(OwnerConstants.CbbOwnerId, result);
    }

    [Fact]
    public async Task AdjustOwnerIds_CbbUser_AddsCbbOwner()
    {
        _denmarkApi.IsUserAsync("user1", 1, "token").Returns(false);
        _denmarkApi.IsCbbUserAsync("user1", 1, "token").Returns(true);

        var result = await _sut.AdjustOwnerIds([1], "user1", 1, "token");

        Assert.DoesNotContain(OwnerConstants.DenmarkOwnerId, result);
        Assert.Contains(OwnerConstants.CbbOwnerId, result);
    }

    [Fact]
    public async Task AdjustOwnerIds_DenmarkAlreadyPresent_NoDuplicate()
    {
        _denmarkApi.IsUserAsync("user1", 1, "token").Returns(true);
        _denmarkApi.IsCbbUserAsync("user1", 1, "token").Returns(false);

        var result = await _sut.AdjustOwnerIds([1, OwnerConstants.DenmarkOwnerId], "user1", 1, "token");

        Assert.Single(result, id => id == OwnerConstants.DenmarkOwnerId);
    }
}

public class DataSubjectRightsServiceTests
{
    private readonly IDenmarkApiClient _denmarkApi = Substitute.For<IDenmarkApiClient>();
    private readonly IZendeskClient _zendeskClient = Substitute.For<IZendeskClient>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly UserDataCacheService _cacheService;
    private readonly DataSubjectRightsService _sut;

    public DataSubjectRightsServiceTests()
    {
        var cacheQueries = Substitute.For<ICacheQueries>();
        _cacheService = new UserDataCacheService(cacheQueries);
        _sut = new DataSubjectRightsService(
            _denmarkApi, _zendeskClient, _emailService,
            _cacheService, NullLogger<DataSubjectRightsService>.Instance);
    }

    [Fact]
    public async Task GetOwnerIds_NoToken_ReturnsTdOnly()
    {
        var result = await _sut.GetOwnerIds("user1", 1, null);

        Assert.Single(result);
        Assert.Contains(OwnerConstants.TdOwnerId, result);
    }

    [Fact]
    public async Task GetOwnerIds_DenmarkUser_IncludesDenmark()
    {
        _denmarkApi.IsUserAsync("user1", 1, "token").Returns(true);
        _denmarkApi.IsCbbUserAsync("user1", 1, "token").Returns(false);

        var result = await _sut.GetOwnerIds("user1", 1, "token");

        Assert.Contains(OwnerConstants.TdOwnerId, result);
        Assert.Contains(OwnerConstants.DenmarkOwnerId, result);
        Assert.DoesNotContain(OwnerConstants.CbbOwnerId, result);
    }

    [Fact]
    public void DefaultDsrStatusMap_ReturnsAllFalse()
    {
        var map = _sut.DefaultDsrStatusMap(OwnerConstants.TdOwnerId);

        Assert.All(map.Values, v => Assert.False(v));
        Assert.NotEmpty(map);
    }

    [Fact]
    public void CreateOwnerMap_MergesCachedValues()
    {
        var cached = new Dictionary<int, Dictionary<string, bool>>
        {
            [OwnerConstants.TdOwnerId] = new() { ["export"] = true }
        };

        var result = _sut.CreateOwnerMap([OwnerConstants.TdOwnerId], cached);

        Assert.Single(result);
        Assert.Equal(OwnerConstants.TdOwnerId, result[0].OwnerId);
        Assert.True(result[0].ReqStates["export"]);
    }

    [Fact]
    public async Task CreateDsrRequest_DenmarkOwner_UsesDenmarkApi()
    {
        _denmarkApi.CreateUserRequestAsync(1, "token", Arg.Any<DenmarkCreateRequestParams>())
            .Returns("dk-ticket-123");

        var result = await _sut.CreateDsrRequest(
            OwnerConstants.DenmarkOwnerId, "user1", 1, "token", "test@test.com", "export", null);

        Assert.Equal("dk-ticket-123", result);
    }

    [Fact]
    public async Task CreateDsrRequest_TdOwnerExport_UsesZendesk()
    {
        _zendeskClient.CreateTicketAsync(Arg.Any<string>(), Arg.Any<string>(), "test@test.com")
            .Returns("zd-ticket-456");

        var result = await _sut.CreateDsrRequest(
            OwnerConstants.TdOwnerId, "user1", 1, null, "test@test.com", DsrConstants.ExportRequestType, null);

        Assert.Equal("zd-ticket-456", result);
    }

    [Fact]
    public async Task CreateDsrRequest_TdOwnerObjection_UsesEmail()
    {
        var result = await _sut.CreateDsrRequest(
            OwnerConstants.TdOwnerId, "user1", 1, null, "test@test.com", DsrConstants.ObjectionRequestType, "note");

        Assert.NotNull(result);
        Assert.StartsWith("email-", result);
        await _emailService.Received(1).SendDsrNotificationEmailAsync("user1", "test@test.com", DsrConstants.ObjectionRequestType, "note");
    }

    [Fact]
    public async Task GetPersonalDataLinks_ReturnsLinksPerOwner()
    {
        _denmarkApi.GetFinishedRequestsAsync("user1", 1, "token")
            .Returns(new List<DenmarkFinishedRequest>());
        _denmarkApi.GetFileSharingLinks(Arg.Any<List<DenmarkFinishedRequest>>(), "user1", 1, "token", "en")
            .Returns(new DenmarkFileSharingResult { Links = [] });

        var result = await _sut.GetPersonalDataLinks(
            [OwnerConstants.DenmarkOwnerId], "user1", 1, "token", "en");

        Assert.Single(result);
        Assert.Equal(OwnerConstants.DenmarkOwnerId, result[0].OwnerId);
    }
}
