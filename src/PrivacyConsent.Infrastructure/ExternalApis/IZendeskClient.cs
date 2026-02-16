namespace PrivacyConsent.Infrastructure.ExternalApis;

public interface IZendeskClient
{
    Task<string?> CreateTicketAsync(string subject, string body, string requesterEmail);
    Task<Dictionary<string, object>> GetRequestStatusesAsync(string email, string ownerTag);
    Task<List<ZendeskFileInfo>> GetPersonalDataFilesAsync(string userId);
    ZendeskFileSharingResult GetFileSharingLinks(List<ZendeskFileInfo> data);
}

public class ZendeskFileInfo
{
    public string TicketId { get; set; } = "";
    public string FileName { get; set; } = "";
    public string Link { get; set; } = "";
    public string? LastModified { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
}

public class ZendeskFileSharingResult
{
    public List<ZendeskFileLink> Links { get; set; } = [];
}

public class ZendeskFileLink
{
    public string TicketId { get; set; } = "";
    public ZendeskFileProduct Product { get; set; } = new();
    public string FileName { get; set; } = "";
    public string Link { get; set; } = "";
    public string? LastModified { get; set; }
}

public class ZendeskFileProduct
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
