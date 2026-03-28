using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Financial.Interfaces;
using WAssis.Domain.Modules.Financial.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Financial.Repositories;

public sealed class CommissionReceiptRepository(WAssisDbContext dbContext) : ICommissionReceiptRepository
{
    public async Task AddReceiptAsync(CommissionReceipt receipt, CancellationToken cancellationToken)
    {
        await dbContext.CommissionReceipts.AddAsync(receipt, cancellationToken);
    }

    public async Task AddReconciliationAsync(CommissionReconciliation reconciliation, CancellationToken cancellationToken)
    {
        await dbContext.CommissionReconciliations.AddAsync(reconciliation, cancellationToken);
    }

    public Task<CommissionReconciliation?> GetReconciliationByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.CommissionReconciliations.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
