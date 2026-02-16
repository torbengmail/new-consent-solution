namespace PrivacyConsent.Infrastructure.ExternalApis;

public interface IDenmarkApiClient
{
    Task<DenmarkUserInfo?> GetUserInfoAsync(string userId, int idTypeId, string connectIdToken);
    Task<bool> IsUserAsync(string userId, int idTypeId, string connectIdToken);
    Task<bool> IsCbbUserAsync(string userId, int idTypeId, string connectIdToken);
    Task<bool> IsNemIdValidatedAsync(string userId, int idTypeId, string connectIdToken);
    Task<List<DenmarkDsrRequest>> GetUserRequestsAsync(string userId, int idTypeId, string connectIdToken, string requestType);
    Task<string?> CreateUserRequestAsync(int idTypeId, string connectIdToken, DenmarkCreateRequestParams request);
    Task<Dictionary<string, object>> GetPendingRequestsAsync(string userId, int idTypeId, string connectIdToken);
    Task<List<DenmarkFinishedRequest>> GetFinishedRequestsAsync(string userId, int idTypeId, string connectIdToken);
    DenmarkFileSharingResult GetFileSharingLinks(List<DenmarkFinishedRequest> data, string userId, int idTypeId, string connectIdToken, string language);
}

public class DenmarkUserInfo
{
    public bool IsUser { get; set; }
    public bool? MinorFlag { get; set; }
    public bool NemIdValidated { get; set; }
}

public class DenmarkDsrRequest
{
    public string? Id { get; set; }
    public string? Step { get; set; }
    public string? StepStatus { get; set; }
}

public class DenmarkCreateRequestParams
{
    public string UserId { get; set; } = "";
    public string? Email { get; set; }
    public string Right { get; set; } = "";
    public string? Note { get; set; }
}

public class DenmarkFinishedRequest
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
}

public class DenmarkFileSharingResult
{
    public List<DenmarkFileLink> Links { get; set; } = [];
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DenmarkFileLink
{
    public string TicketId { get; set; } = "";
    public DenmarkFileProduct Product { get; set; } = new();
    public string FileName { get; set; } = "";
    public string Link { get; set; } = "";
    public string? LastModified { get; set; }
}

public class DenmarkFileProduct
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
