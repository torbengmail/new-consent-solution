namespace PrivacyConsent.Domain.Constants;

public static class DsrConstants
{
    public const string ObjectionRequestType = "objection";
    public const string ExportRequestType = "export";
    public const string RectificationRequestType = "rectification";
    public const string RestrictionRequestType = "restriction";
    public const string DeleteRequestType = "deletion";
    public const string PortabilityRequestType = "portability";
    public const string TerminationRequestType = "termination";

    public const string NoTicket = "no-ticket";
    public const string DsrAutoDeletionTicketId = "auto-deletion";
    public const int PrivacyDashboardSourceId = 6;

    public static readonly Dictionary<string, string> RequestTypeLabels = new()
    {
        [ExportRequestType] = "Access Information",
        [RectificationRequestType] = "Rectification",
        [DeleteRequestType] = "Erasure",
        [RestrictionRequestType] = "Restrict Processing",
        [ObjectionRequestType] = "Other",
    };

    public static readonly Dictionary<int, string[]> DsrTypesByOwner = new()
    {
        [OwnerConstants.TdOwnerId] = [ObjectionRequestType, RectificationRequestType, DeleteRequestType, ExportRequestType, TerminationRequestType],
        [OwnerConstants.DenmarkOwnerId] = [ObjectionRequestType, RestrictionRequestType, DeleteRequestType, PortabilityRequestType, ExportRequestType],
        [OwnerConstants.CbbOwnerId] = [ObjectionRequestType, RestrictionRequestType, DeleteRequestType, PortabilityRequestType, ExportRequestType],
    };

    public static string[] GetDsrTypes(int ownerId) =>
        DsrTypesByOwner.TryGetValue(ownerId, out var types) ? types : DsrTypesByOwner[OwnerConstants.TdOwnerId];
}
