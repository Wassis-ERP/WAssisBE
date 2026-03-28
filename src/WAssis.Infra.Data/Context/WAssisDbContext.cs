using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Domain.Core.Auditing;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Financial.Entities;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Infra.Data.Context;

public class WAssisDbContext : DbContext
{
    private const string MissingTenantSentinel = "__missing_authenticated_tenant__";
    private readonly string? _currentTenantId;

    public WAssisDbContext(
        DbContextOptions<WAssisDbContext> options,
        ICurrentUserContext currentUserContext)
        : base(options)
    {
        _currentTenantId = ResolveTenantScope(currentUserContext);
    }

    public DbSet<DocumentSearch> DocumentSearches => Set<DocumentSearch>();
    public DbSet<ImportedDocument> ImportedDocuments => Set<ImportedDocument>();
    public DbSet<CommissionReceipt> CommissionReceipts => Set<CommissionReceipt>();
    public DbSet<CommissionReconciliation> CommissionReconciliations => Set<CommissionReconciliation>();
    public DbSet<PolicyDraft> PolicyDrafts => Set<PolicyDraft>();
    public DbSet<QuoteRequest> QuoteRequests => Set<QuoteRequest>();
    public DbSet<QuoteOption> QuoteOptions => Set<QuoteOption>();
    public DbSet<WhatsAppConversation> WhatsAppConversations => Set<WhatsAppConversation>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WAssisDbContext).Assembly);

        modelBuilder.Entity<DocumentSearch>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<ImportedDocument>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<CommissionReceipt>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<CommissionReconciliation>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<PolicyDraft>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<QuoteRequest>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<QuoteOption>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<WhatsAppConversation>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
        modelBuilder.Entity<AuditEntry>().HasQueryFilter(x => _currentTenantId == null || x.TenantId == _currentTenantId);
    }

    private static string? ResolveTenantScope(ICurrentUserContext currentUserContext)
    {
        if (!string.IsNullOrWhiteSpace(currentUserContext.TenantId))
        {
            return currentUserContext.TenantId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(currentUserContext.BrokerageId))
        {
            return currentUserContext.BrokerageId.Trim();
        }

        if (currentUserContext.IsAuthenticated)
        {
            return MissingTenantSentinel;
        }

        return null;
    }
}
