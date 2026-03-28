using WAssis.Domain.Modules.Financial.Entities;

namespace WAssis.Application.Modules.Financial.Interfaces;

public interface ICommissionReceiptRepository
{
    Task AddReceiptAsync(CommissionReceipt receipt, CancellationToken cancellationToken);
    Task AddReconciliationAsync(CommissionReconciliation reconciliation, CancellationToken cancellationToken);
    Task<CommissionReconciliation?> GetReconciliationByIdAsync(Guid id, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
