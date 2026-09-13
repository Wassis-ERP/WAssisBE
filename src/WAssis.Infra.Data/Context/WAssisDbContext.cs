using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Domain.Core.Auditing;
using WAssis.Domain.Modules.Billing.Entities;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Financial.Entities;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Infra.Data.Context;

public class WAssisDbContext : DbContext
{
    private const string MissingTenantSentinel = "__missing_authenticated_tenant__";
    private readonly string? _currentTenantId;
    private readonly bool _hasAllBranchesAccess;
    private readonly string[] _branchIds;

    public WAssisDbContext(
        DbContextOptions<WAssisDbContext> options,
        ICurrentUserContext currentUserContext,
        SystemDataScope? systemScope = null)
        : base(options)
    {
        _currentTenantId = systemScope is not null && !currentUserContext.IsAuthenticated
            ? null : ResolveTenantScope(currentUserContext);
        _hasAllBranchesAccess = currentUserContext.HasAllBranchesAccess;
        _branchIds = ResolveBranchScope(currentUserContext);
    }

    public DbSet<DocumentSearch> DocumentSearches => Set<DocumentSearch>();
    public DbSet<ImportedDocument> ImportedDocuments => Set<ImportedDocument>();
    public DbSet<InsuredPerson> InsuredPeople => Set<InsuredPerson>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<BillingSubscription> BillingSubscriptions => Set<BillingSubscription>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();
    public DbSet<CommissionReceipt> CommissionReceipts => Set<CommissionReceipt>();
    public DbSet<CommissionReconciliation> CommissionReconciliations => Set<CommissionReconciliation>();
    public DbSet<PolicyDraft> PolicyDrafts => Set<PolicyDraft>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<QuoteOption> QuoteOptions => Set<QuoteOption>();
    public DbSet<QuoteProviderActivationSetting> QuoteProviderActivationSettings => Set<QuoteProviderActivationSetting>();
    public DbSet<WhatsAppConversation> WhatsAppConversations => Set<WhatsAppConversation>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WAssisDbContext).Assembly);

        // Include ownership in UPDATE/DELETE predicates, including entities attached without reading.
        // This is EF command metadata; it adds no database columns.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tenant = entity.FindProperty("TenantId");
            if (tenant is not null) tenant.IsConcurrencyToken = true;
            var branch = entity.FindProperty("OfficeBranchId");
            if (branch is not null) branch.IsConcurrencyToken = true;
        }

        modelBuilder.Entity<DocumentSearch>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<ImportedDocument>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<InsuredPerson>().HasQueryFilter(x =>
            (_currentTenantId == null || x.TenantId == _currentTenantId) &&
            (_currentTenantId == null || _hasAllBranchesAccess || (x.OfficeBranchId != null && _branchIds.Contains(x.OfficeBranchId))));
        modelBuilder.Entity<Opportunity>().HasQueryFilter(x =>
            (_currentTenantId == null || x.TenantId == _currentTenantId) &&
            (_currentTenantId == null || _hasAllBranchesAccess || (x.OfficeBranchId != null && _branchIds.Contains(x.OfficeBranchId))));
        modelBuilder.Entity<BillingSubscription>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<BillingInvoice>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<CommissionReceipt>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<CommissionReconciliation>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<PolicyDraft>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<QuoteRequest>().HasQueryFilter(x =>
            (_currentTenantId == null || x.TenantId == _currentTenantId) &&
            (_currentTenantId == null || _hasAllBranchesAccess || (x.OfficeBranchId != null && _branchIds.Contains(x.OfficeBranchId))));
        modelBuilder.Entity<QuoteOption>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<WhatsAppConversation>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<AuditEntry>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
    }

    private static string? ResolveTenantScope(ICurrentUserContext currentUserContext)
    {
        if (currentUserContext.IsAuthenticated && !string.IsNullOrWhiteSpace(currentUserContext.TenantId))
        {
            return currentUserContext.TenantId.Trim();
        }

        return MissingTenantSentinel;
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateWriteScope();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateWriteScope();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateWriteScope()
    {
        if (_currentTenantId is null) return; // Only an explicit, audited SystemDataScope grants this capability.
        foreach (var entry in ChangeTracker.Entries().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Metadata.FindProperty("TenantId") is null) continue;
            var tenant = entry.Property("TenantId");
            if (_currentTenantId == MissingTenantSentinel || !Equals(tenant.CurrentValue, _currentTenantId)
                || (entry.State != EntityState.Added && !Equals(tenant.OriginalValue, _currentTenantId)))
                throw new UnauthorizedAccessException("A escrita exige o mesmo tenant do contexto autenticado.");
            if (!_hasAllBranchesAccess && entry.Metadata.FindProperty("OfficeBranchId") is not null)
            {
                var branch = entry.Property("OfficeBranchId");
                if (branch.CurrentValue is not string current || !_branchIds.Contains(current)
                    || (entry.State != EntityState.Added && (branch.OriginalValue is not string original || !_branchIds.Contains(original))))
                    throw new UnauthorizedAccessException("A escrita exige uma filial autorizada.");
            }
        }
    }

    private static string[] ResolveBranchScope(ICurrentUserContext currentUserContext)
    {
        return currentUserContext.BranchIds
            .Append(currentUserContext.BranchId)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
