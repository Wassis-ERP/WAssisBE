using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Application.Modules.Quotes.Dtos;

public sealed record QuoteProviderResultDto(
    string ProviderCode,
    string ProviderName,
    QuoteOptionStatus Status,
    string ExternalReference,
    decimal? PremiumAmount,
    decimal? CommissionAmount,
    IReadOnlyCollection<CoverageSnapshotDto> Coverages,
    IReadOnlyCollection<InstallmentSnapshotDto> Installments,
    IReadOnlyCollection<QuoteStatusMessageDto> Messages);

public sealed record QuoteProviderRequirementDto(
    string Code,
    string Description,
    bool IsConfigured);

public sealed record QuoteProviderDescriptorDto(
    string ProviderCode,
    string ProviderName,
    bool IsEnabled,
    bool IsReady,
    string AuthMode,
    string? ProductLine,
    string? OfficialDocumentationUrl,
    IReadOnlyCollection<QuoteProviderRequirementDto> Requirements,
    IReadOnlyCollection<QuoteStatusMessageDto> Messages);
