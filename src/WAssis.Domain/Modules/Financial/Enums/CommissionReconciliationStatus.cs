namespace WAssis.Domain.Modules.Financial.Enums;

public enum CommissionReconciliationStatus
{
    Pending = 0,
    Matched = 1,
    Divergent = 2,
    Settled = 3,
    Rejected = 4
}
