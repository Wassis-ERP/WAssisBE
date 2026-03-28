using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Quotes.Enums;
using WAssis.Domain.Modules.Quotes.ValueObjects;

namespace WAssis.Domain.Modules.Quotes.Entities;

public class QuoteOption : Entity
{
    private readonly List<CoverageSnapshot> _coverages = [];
    private readonly List<InstallmentSnapshot> _installments = [];
    private readonly List<QuoteStatusMessage> _messages = [];

    public Guid QuoteRequestId { get; private set; }
    public string InsuranceCompanyCode { get; private set; } = string.Empty;
    public string InsuranceCompanyName { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public QuoteOptionStatus Status { get; private set; }
    public decimal? PremiumAmount { get; private set; }
    public decimal? CommissionAmount { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public IReadOnlyCollection<CoverageSnapshot> Coverages => _coverages;
    public IReadOnlyCollection<InstallmentSnapshot> Installments => _installments;
    public IReadOnlyCollection<QuoteStatusMessage> Messages => _messages;

    private QuoteOption()
    {
    }

    public QuoteOption(
        Guid id,
        Guid quoteRequestId,
        string insuranceCompanyCode,
        string insuranceCompanyName,
        string productCode,
        string productName,
        QuoteOptionStatus status,
        decimal? premiumAmount,
        decimal? commissionAmount,
        string externalReference,
        IEnumerable<CoverageSnapshot>? coverages,
        IEnumerable<InstallmentSnapshot>? installments,
        IEnumerable<QuoteStatusMessage>? messages)
    {
        Id = id;
        QuoteRequestId = quoteRequestId;
        InsuranceCompanyCode = insuranceCompanyCode;
        InsuranceCompanyName = insuranceCompanyName;
        ProductCode = productCode;
        ProductName = productName;
        Status = status;
        PremiumAmount = premiumAmount;
        CommissionAmount = commissionAmount;
        ExternalReference = externalReference;

        if (coverages is not null)
        {
            _coverages.AddRange(coverages);
        }

        if (installments is not null)
        {
            _installments.AddRange(installments);
        }

        if (messages is not null)
        {
            _messages.AddRange(messages);
        }
    }
}
