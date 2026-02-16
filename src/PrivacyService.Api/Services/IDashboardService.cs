namespace PrivacyService.Api.Services;

public interface IDashboardService
{
    Task<List<int>> AdjustOwnerIds(List<int> ownerIds, string userId, int idTypeId, string? accessToken);
}
