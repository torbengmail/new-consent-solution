using PrivacyConsent.Data.Queries;
using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Infrastructure.ExternalApis;

namespace PrivacyService.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IExpressionQueries _expressionQueries;
    private readonly IDenmarkApiClient _denmarkApi;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IExpressionQueries expressionQueries,
        IDenmarkApiClient denmarkApi,
        ILogger<DashboardService> logger)
    {
        _expressionQueries = expressionQueries;
        _denmarkApi = denmarkApi;
        _logger = logger;
    }

    public async Task<List<int>> AdjustOwnerIds(List<int> ownerIds, string userId, int idTypeId, string? accessToken)
    {
        var result = new List<int>(ownerIds);

        if (accessToken != null)
        {
            if (await _denmarkApi.IsUserAsync(userId, idTypeId, accessToken))
            {
                if (!result.Contains(OwnerConstants.DenmarkOwnerId))
                    result.Add(OwnerConstants.DenmarkOwnerId);
            }

            if (await _denmarkApi.IsCbbUserAsync(userId, idTypeId, accessToken))
            {
                if (!result.Contains(OwnerConstants.CbbOwnerId))
                    result.Add(OwnerConstants.CbbOwnerId);
            }
        }

        return result.Distinct().ToList();
    }
}
