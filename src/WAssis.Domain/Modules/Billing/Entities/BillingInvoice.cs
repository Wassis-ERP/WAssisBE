using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Domain.Modules.Billing.Entities;

public class BillingInvoice : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid BillingSubscriptionId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string ReferencePeriod { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public DateTime DueDateUtc { get; private set; }
    public BillingInvoiceStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? Notes { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private BillingInvoice()
    {
    }

    private BillingInvoice(
        Guid id,
        string tenantId,
        Guid billingSubscriptionId,
        string correlationId,
        string referencePeriod,
        decimal amount,
        DateTime dueDateUtc,
        string? externalReference,
        string? notes)
    {
        Id = id;
        TenantId = tenantId;
        BillingSubscriptionId = billingSubscriptionId;
        CorrelationId = correlationId;
        ReferencePeriod = referencePeriod;
        Amount = amount;
        DueDateUtc = dueDateUtc;
        ExternalReference = externalReference;
        Notes = notes;
        Status = BillingInvoiceStatus.Pending;
        IssuedAtUtc = DateTime.UtcNow;
    }

    public static BillingInvoice Create(
        string tenantId,
        Guid billingSubscriptionId,
        string correlationId,
        string referencePeriod,
        decimal amount,
        DateTime dueDateUtc,
        string? externalReference,
        string? notes)
    {
        return new BillingInvoice(
            Guid.NewGuid(),
            tenantId,
            billingSubscriptionId,
            correlationId,
            referencePeriod,
            amount,
            dueDateUtc,
            externalReference,
            notes);
    }

    public void MarkPaid(string? paymentMethod, string? externalReference, string? notes)
    {
        Status = BillingInvoiceStatus.Paid;
        PaymentMethod = paymentMethod;
        ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? ExternalReference : externalReference;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes;
        PaidAtUtc = DateTime.UtcNow;
    }
}
