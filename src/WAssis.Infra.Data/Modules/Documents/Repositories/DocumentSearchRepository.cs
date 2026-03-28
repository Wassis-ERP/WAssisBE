using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Documents.Repositories;

public sealed class DocumentSearchRepository(WAssisDbContext dbContext) : IDocumentSearchRepository
{
    public async Task AddAsync(DocumentSearch search, CancellationToken cancellationToken)
    {
        await dbContext.DocumentSearches.AddAsync(search, cancellationToken);
    }

    public Task<DocumentSearch?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.DocumentSearches.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<DocumentSearch?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken)
    {
        return dbContext.DocumentSearches.SingleOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
