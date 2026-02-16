using Microsoft.EntityFrameworkCore;
using PrivacyConsent.Data.Entities;

namespace PrivacyConsent.Data;

public class PrivacyDbContext : DbContext
{
    public PrivacyDbContext(DbContextOptions<PrivacyDbContext> options) : base(options) { }

    // Consent schema
    public DbSet<Consent> Consents => Set<Consent>();
    public DbSet<ConsentType> ConsentTypes => Set<ConsentType>();
    public DbSet<ConsentExpression> ConsentExpressions => Set<ConsentExpression>();
    public DbSet<ConsentExpressionStatus> ConsentExpressionStatuses => Set<ConsentExpressionStatus>();
    public DbSet<ConsentExpressionText> ConsentExpressionTexts => Set<ConsentExpressionText>();
    public DbSet<ConsentExpressionTag> ConsentExpressionTags => Set<ConsentExpressionTag>();
    public DbSet<ConsentExpressionTagList> ConsentExpressionTagLists => Set<ConsentExpressionTagList>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<UserConsentAuditTrail> UserConsentAuditTrails => Set<UserConsentAuditTrail>();
    public DbSet<RequestAttempt> RequestAttempts => Set<RequestAttempt>();
    public DbSet<RequestAttemptAuditTrail> RequestAttemptAuditTrails => Set<RequestAttemptAuditTrail>();
    public DbSet<UserConsentSource> UserConsentSources => Set<UserConsentSource>();
    public DbSet<UserConsentSourceType> UserConsentSourceTypes => Set<UserConsentSourceType>();
    public DbSet<MasterId> MasterIds => Set<MasterId>();
    public DbSet<IdType> IdTypes => Set<IdType>();
    public DbSet<IdMap> IdMaps => Set<IdMap>();
    public DbSet<AdminTranslation> AdminTranslations => Set<AdminTranslation>();
    public DbSet<UserDataCache> UserDataCaches => Set<UserDataCache>();
    public DbSet<DsrTracking> DsrTrackings => Set<DsrTracking>();
    public DbSet<UserConsentDecisionsBatch> UserConsentDecisionsBatches => Set<UserConsentDecisionsBatch>();
    public DbSet<Skin> Skins => Set<Skin>();
    public DbSet<TestUserGroup> TestUserGroups => Set<TestUserGroup>();
    public DbSet<TestUser> TestUsers => Set<TestUser>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserOwner> UserOwners => Set<UserOwner>();
    public DbSet<AdminApiAuditTrail> AdminApiAuditTrails => Set<AdminApiAuditTrail>();
    public DbSet<UseCaseConsent> UseCaseConsents => Set<UseCaseConsent>();
    public DbSet<ProductConnectId> ProductConnectIds => Set<ProductConnectId>();

    // Data Inventory schema
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PurposeCategory> PurposeCategories => Set<PurposeCategory>();
    public DbSet<UseCase> UseCases => Set<UseCase>();
    public DbSet<LegalBasis> LegalBases => Set<LegalBasis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ConsentExpressionText composite key
        modelBuilder.Entity<ConsentExpressionText>()
            .HasKey(e => new { e.ConsentExpressionId, e.Language });

        modelBuilder.Entity<ConsentExpressionText>()
            .HasOne(e => e.ConsentExpression)
            .WithMany(e => e.Texts)
            .HasForeignKey(e => e.ConsentExpressionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ConsentExpressionTagList composite key
        modelBuilder.Entity<ConsentExpressionTagList>()
            .HasKey(e => new { e.ConsentExpressionId, e.ConsentExpressionTagId });

        modelBuilder.Entity<ConsentExpressionTagList>()
            .HasOne(e => e.ConsentExpression)
            .WithMany(e => e.TagList)
            .HasForeignKey(e => e.ConsentExpressionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConsentExpressionTagList>()
            .HasOne(e => e.Tag)
            .WithMany()
            .HasForeignKey(e => e.ConsentExpressionTagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Consent relationships
        modelBuilder.Entity<Consent>()
            .HasOne(e => e.ConsentType)
            .WithMany()
            .HasForeignKey(e => e.ConsentTypeId);

        modelBuilder.Entity<ConsentExpression>()
            .HasOne(e => e.Consent)
            .WithMany(e => e.Expressions)
            .HasForeignKey(e => e.ConsentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConsentExpression>()
            .HasOne(e => e.Status)
            .WithMany()
            .HasForeignKey(e => e.StatusId);

        // MasterId uses application-generated UUIDs
        modelBuilder.Entity<MasterId>()
            .Property(e => e.Id)
            .ValueGeneratedNever();

        // UserConsent relationships
        modelBuilder.Entity<UserConsent>()
            .HasOne(e => e.Consent)
            .WithMany(e => e.UserConsents)
            .HasForeignKey(e => e.ConsentId);

        modelBuilder.Entity<UserConsent>()
            .HasOne(e => e.Master)
            .WithMany()
            .HasForeignKey(e => e.MasterId);

        modelBuilder.Entity<UserConsentAuditTrail>()
            .HasOne(e => e.Decision)
            .WithMany(e => e.AuditTrails)
            .HasForeignKey(e => e.DecisionId);

        // UserDataCache composite key
        modelBuilder.Entity<UserDataCache>()
            .HasKey(e => new { e.UserId, e.IdTypeId, e.DataKey });

        // DsrTracking composite key
        modelBuilder.Entity<DsrTracking>()
            .HasKey(e => new { e.TicketId, e.UserId, e.IdTypeId, e.Type });

        // TestUser composite key
        modelBuilder.Entity<TestUser>()
            .HasKey(e => new { e.GroupId, e.UserId, e.IdTypeId });

        // RBAC
        modelBuilder.Entity<RolePermission>()
            .HasKey(e => new { e.RoleId, e.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasOne(e => e.Role)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(e => e.Permission)
            .WithMany()
            .HasForeignKey(e => e.PermissionId);

        modelBuilder.Entity<UserRole>()
            .HasKey(e => new { e.UserId, e.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserOwner>()
            .HasKey(e => new { e.UserId, e.OwnerId });

        modelBuilder.Entity<UserOwner>()
            .HasOne(e => e.User)
            .WithMany(e => e.UserOwners)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UseCaseConsent composite key
        modelBuilder.Entity<UseCaseConsent>()
            .HasKey(e => new { e.UseCaseId, e.ConsentId });

        modelBuilder.Entity<UseCaseConsent>()
            .HasOne(e => e.Consent)
            .WithMany()
            .HasForeignKey(e => e.ConsentId);

        // Global soft-delete filter: automatically excludes deleted consents from all queries
        modelBuilder.Entity<Consent>()
            .HasQueryFilter(c => c.DeleteAt == null);

        // Data Inventory
        modelBuilder.Entity<Product>()
            .HasOne(e => e.Owner)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.OwnerId);
    }
}
