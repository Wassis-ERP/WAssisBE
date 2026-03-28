using WAssis.Domain.Core.Entities;

namespace WAssis.Domain.Modules.Financial.Entities;

public class CommissionReceipt : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string InsuranceCompanyCode { get; private set; } = string.Empty;
    public decimal ReceivedAmount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public string SourceType { get; private set; } = string.Empty;
    public Guid? ImportedDocumentId { get; private set; }
    public DateTime ReceivedAtUtc { get; private set; }

    private CommissionReceipt()
    {
    }

    private CommissionReceipt(
        Guid id,
        string tenantId,
        string correlationId,
        string insuranceCompanyCode,
        decimal receivedAmount,
        string sourceType,
        Guid? importedDocumentId)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        InsuranceCompanyCode = insuranceCompanyCode;
        ReceivedAmount = receivedAmount;
        SourceType = sourceType;
        ImportedDocumentId = importedDocumentId;
        ReceivedAtUtc = DateTime.UtcNow;
    }

    public static CommissionReceipt Create(
        string tenantId,
        string correlationId,
        string insuranceCompanyCode,
        decimal receivedAmount,
        string sourceType,
        Guid? importedDocumentId)
    {
        return new CommissionReceipt(Guid.NewGuid(), tenantId, correlationId, insuranceCompanyCode, receivedAmount, sourceType, importedDocumentId);
    }
}
