namespace PrivacyConsent.Domain.DTOs.Common;

public class ReferenceDataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class OwnedReferenceDataItem : ReferenceDataItem
{
    public int? OwnerId { get; set; }
}
