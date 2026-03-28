using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Financial.Enums;

namespace WAssis.Domain.Modules.Financial.Entities;

public class CommissionReconciliation : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid CommissionReceiptId { get; private set; }
    public decimal ExpectedAmount { get; private set; }
    public decimal ReceivedAmount { get; private set; }
    public decimal DifferenceAmount { get; private set; }
    public CommissionReconciliationStatus Status { get; private set; }
    public string? MatchedReference { get; private set; }
    public string? SettlementNotes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? SettledAtUtc { get; private set; }

    private CommissionReconciliation()
    {
    }

    private CommissionReconciliation(Guid id, string tenantId, Guid commissionReceiptId, decimal expectedAmount, decimal receivedAmount)
    {
        Id = id;
        TenantId = tenantId;
        CommissionReceiptId = commissionReceiptId;
        ExpectedAmount = expectedAmount;
        ReceivedAmount = receivedAmount;
        DifferenceAmount = receivedAmount - expectedAmount;
        Status = DifferenceAmount == 0 ? CommissionReconciliationStatus.Matched : CommissionReconciliationStatus.Divergent;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static CommissionReconciliation Create(string tenantId, Guid commissionReceiptId, decimal expectedAmount, decimal receivedAmount)
    {
        return new CommissionReconciliation(Guid.NewGuid(), tenantId, commissionReceiptId, expectedAmount, receivedAmount);
    }

    public void MarkSettled(string? matchedReference, string? settlementNotes)
    {
        Status = CommissionReconciliationStatus.Settled;
        MatchedReference = matchedReference;
        SettlementNotes = settlementNotes;
        SettledAtUtc = DateTime.UtcNow;
    }
}
