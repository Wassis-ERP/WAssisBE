using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Documents.Enums;

namespace WAssis.Domain.Modules.Documents.Entities;

public class DocumentSearch : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string InsuranceCompanyCode { get; private set; } = string.Empty;
    public string SearchType { get; private set; } = string.Empty;
    public DocumentSearchStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private DocumentSearch()
    {
    }

    private DocumentSearch(Guid id, string tenantId, string correlationId, string insuranceCompanyCode, string searchType)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        InsuranceCompanyCode = insuranceCompanyCode;
        SearchType = searchType;
        Status = DocumentSearchStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static DocumentSearch Create(string tenantId, string correlationId, string insuranceCompanyCode, string searchType)
    {
        return new DocumentSearch(Guid.NewGuid(), tenantId, correlationId, insuranceCompanyCode, searchType);
    }
}
