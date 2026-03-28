using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record QuoteOptionDto(
    Guid Id,
    string InsuranceCompanyCode,
    string InsuranceCompanyName,
    string ProductCode,
    string ProductName,
    QuoteOptionStatus Status,
    decimal? PremiumAmount,
    decimal? CommissionAmount,
    string ExternalReference,
    IReadOnlyCollection<CoverageSnapshotDto> Coverages,
    IReadOnlyCollection<InstallmentSnapshotDto> Installments,
    IReadOnlyCollection<QuoteStatusMessageDto> Messages);
