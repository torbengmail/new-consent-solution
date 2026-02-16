using PrivacyConsent.Domain.Constants;
using PrivacyConsent.Domain.DTOs.ServiceApi;
using PrivacyConsent.Infrastructure.Cache;
using PrivacyConsent.Infrastructure.ExternalApis;
using PrivacyConsent.Infrastructure.Email;

namespace PrivacyService.Api.Services;

public class DataSubjectRightsService : IDataSubjectRightsService
{
    private readonly IDenmarkApiClient _denmarkApi;
    private readonly IZendeskClient _zendeskClient;
    private readonly IEmailService _emailService;
    private readonly UserDataCacheService _cacheService;
    private readonly ILogger<DataSubjectRightsService> _logger;

    public DataSubjectRightsService(
        IDenmarkApiClient denmarkApi,
        IZendeskClient zendeskClient,
        IEmailService emailService,
        UserDataCacheService cacheService,
        ILogger<DataSubjectRightsService> logger)
    {
        _denmarkApi = denmarkApi;
        _zendeskClient = zendeskClient;
        _emailService = emailService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<int>> GetOwnerIds(string userId, int idTypeId, string? accessToken)
    {
        var owners = new Dictionary<int, bool>
        {
            [OwnerConstants.TdOwnerId] = true,
            [OwnerConstants.DenmarkOwnerId] = accessToken != null &&
                await _denmarkApi.IsUserAsync(userId, idTypeId, accessToken),
            [OwnerConstants.CbbOwnerId] = accessToken != null &&
                await _denmarkApi.IsCbbUserAsync(userId, idTypeId, accessToken)
        };

        return owners.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
    }

    public Dictionary<string, bool> DefaultDsrStatusMap(int ownerId)
    {
        var types = DsrConstants.GetDsrTypes(ownerId);
        return types.ToDictionary(t => t, _ => false);
    }

    public List<DsrRightsDto> CreateOwnerMap(List<int> ownerIds, Dictionary<int, Dictionary<string, bool>> cachedValues)
    {
        return ownerIds.Select(ownerId =>
        {
            var defaults = DefaultDsrStatusMap(ownerId);
            var cached = cachedValues.GetValueOrDefault(ownerId, new Dictionary<string, bool>());
            var merged = new Dictionary<string, bool>(defaults);
            foreach (var (key, value) in cached)
                merged[key] = value;

            return new DsrRightsDto
            {
                OwnerId = ownerId,
                OwnerName = OwnerConstants.GetOwnerName(ownerId) ?? "Unknown",
                ReqStates = merged
            };
        }).ToList();
    }

    public async Task<string?> CreateDsrRequest(
        int ownerId, string userId, int idTypeId, string? accessToken,
        string? email, string right, string? note)
    {
        string? ticketId = null;

        switch (ownerId)
        {
            case OwnerConstants.DenmarkOwnerId:
            case OwnerConstants.CbbOwnerId:
                ticketId = await _denmarkApi.CreateUserRequestAsync(
                    idTypeId, accessToken ?? "",
                    new DenmarkCreateRequestParams
                    {
                        UserId = userId,
                        Email = email,
                        Right = right,
                        Note = note
                    });
                break;

            case OwnerConstants.TdOwnerId:
                if (right is DsrConstants.ObjectionRequestType or DsrConstants.RectificationRequestType or DsrConstants.TerminationRequestType)
                {
                    // Email-based DSR for TD
                    await _emailService.SendDsrNotificationEmailAsync(userId, email ?? "", right, note);
                    ticketId = $"email-{Guid.NewGuid():N}";
                }
                else
                {
                    // Zendesk-based DSR
                    ticketId = await _zendeskClient.CreateTicketAsync(
                        $"DSR {right} request",
                        $"User {userId} requests {right}. Note: {note ?? "N/A"}",
                        email ?? "");
                }
                break;
        }

        if (ticketId != null)
        {
            // Update cache
            var cacheKey = CacheConstants.CreateCompositeKey(ownerId, right);
            await _cacheService.PutValue(userId, idTypeId, cacheKey, ticketId);
        }

        return ticketId;
    }

    public async Task<List<PersonalDataByOwnerDto>> GetPersonalDataLinks(
        List<int> ownerIds, string userId, int idTypeId, string? accessToken, string language)
    {
        var results = new List<PersonalDataByOwnerDto>();

        foreach (var ownerId in ownerIds.OrderByDescending(id => id))
        {
            var ownerName = OwnerConstants.GetOwnerName(ownerId) ?? "Unknown";
            var dto = new PersonalDataByOwnerDto
            {
                OwnerId = ownerId,
                OwnerName = ownerName
            };

            try
            {
                switch (ownerId)
                {
                    case OwnerConstants.DenmarkOwnerId:
                    case OwnerConstants.CbbOwnerId:
                        var finished = await _denmarkApi.GetFinishedRequestsAsync(
                            userId, idTypeId, accessToken ?? "");
                        var fileSharingResult = _denmarkApi.GetFileSharingLinks(
                            finished, userId, idTypeId, accessToken ?? "", language);
                        dto.Links = fileSharingResult.Links.Select(l => new PersonalDataFileDto
                        {
                            TicketId = l.TicketId,
                            Product = new PersonalDataFileProductDto { Id = l.Product.Id, Name = l.Product.Name },
                            FileName = l.FileName,
                            Link = l.Link,
                            LastModified = l.LastModified
                        }).ToList();
                        break;

                    case OwnerConstants.TdOwnerId:
                        var files = await _zendeskClient.GetPersonalDataFilesAsync(userId);
                        var zendeskResult = _zendeskClient.GetFileSharingLinks(files);
                        dto.Links = zendeskResult.Links.Select(l => new PersonalDataFileDto
                        {
                            TicketId = l.TicketId,
                            Product = new PersonalDataFileProductDto { Id = l.Product.Id, Name = l.Product.Name },
                            FileName = l.FileName,
                            Link = l.Link,
                            LastModified = l.LastModified
                        }).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get personal data for owner {OwnerId}", ownerId);
                dto.ErrorMessage = $"Failed to retrieve personal data for {ownerName}";
            }

            results.Add(dto);
        }

        return results;
    }
}
