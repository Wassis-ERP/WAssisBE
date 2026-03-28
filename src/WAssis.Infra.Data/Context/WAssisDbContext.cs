using Microsoft.EntityFrameworkCore;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Financial.Entities;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;
using WAssis.Domain.Core.Auditing;

namespace WAssis.Infra.Data.Context;

public class WAssisDbContext(DbContextOptions<WAssisDbContext> options) : DbContext(options)
{
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
    }
}
