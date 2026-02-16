using PrivacyConsent.Domain.DTOs.ServiceApi;

namespace PrivacyService.Api.Services;

public interface IDataSubjectRightsService
{
    Task<List<int>> GetOwnerIds(string userId, int idTypeId, string? accessToken);
    Dictionary<string, bool> DefaultDsrStatusMap(int ownerId);
    List<DsrRightsDto> CreateOwnerMap(List<int> ownerIds, Dictionary<int, Dictionary<string, bool>> cachedValues);
    Task<string?> CreateDsrRequest(int ownerId, string userId, int idTypeId, string? accessToken, string? email, string right, string? note);
    Task<List<PersonalDataByOwnerDto>> GetPersonalDataLinks(List<int> ownerIds, string userId, int idTypeId, string? accessToken, string language);
}
