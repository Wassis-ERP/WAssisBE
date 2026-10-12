using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Documents.Repositories;

public sealed class ImportedDocumentRepository(WAssisDbContext dbContext) : IImportedDocumentRepository
{
    public async Task AddAsync(ImportedDocument document, CancellationToken cancellationToken)
    {
        await dbContext.ImportedDocuments.AddAsync(document, cancellationToken);
    }

    public Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.ImportedDocuments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var added = dbContext.ChangeTracker.Entries<ImportedDocument>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity)
            .ToArray();
        if (added.Length == 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }
        await using var transaction = dbContext.Database.CurrentTransaction is null
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        await dbContext.SaveChangesAsync(cancellationToken);
        foreach (var document in added)
            await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO infrastructure.work_outbox (id,tenant_id,branch_id,kind,aggregate_id)
                VALUES ({Guid.NewGuid()},{document.TenantId},{document.OfficeBranchId},'documents.parse',{document.Id})
                """, cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
    }
}
