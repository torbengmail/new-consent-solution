namespace TestUtilities;

/// <summary>
/// Constants for test data values seeded in the test database.
/// Shared across all test projects to eliminate magic numbers.
/// </summary>
public static class SeedData
{
    // Credentials
    public const string AdminUsername = "admin";
    public const string AdminPassword = "test";
    public const string UnprivilegedUsername = "test";
    public const string UnprivilegedPassword = "pwd";

    // Seeded user IDs
    public const string TestUserId = "222";
    public const string TestUserId2 = "2222";

    // Seeded consent/expression IDs
    public const int ConsentId = 201;
    public const int ExpressionId = 301;
    public const int ConsentExpressionId111 = 111;

    // Seeded owner/product IDs
    public const int DefaultOwnerId = 1;
    public const int DenmarkOwnerId = 6;
    public const int CaptureProductId = 8;

    // ID type
    public const int ConnectIdType = 1;

    // Audit trail IDs from seed data
    public const long AuditTrailId1 = 9001;
    public const long AuditTrailId2 = 9002;

    // Use case
    public const int UseCaseId = 1001;

    // Decision IDs from seed data
    public const int DecisionId7000 = 7000;

    // User consent source
    public const int UserConsentSourceId = 3;

    // Master GUID for user "222" / id_type 1
    public const string MasterGuid222 = "ecdea009-b706-3365-b882-a13e8386d090";
}
