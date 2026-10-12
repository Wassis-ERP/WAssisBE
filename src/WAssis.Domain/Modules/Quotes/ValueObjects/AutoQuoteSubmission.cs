namespace WAssis.Domain.Modules.Quotes.ValueObjects;

// Immutable submission envelope for the legacy dispatch pipeline, not erp.calculos.
public sealed record AutoQuoteSubmission
{
    public string DriverName { get; init; } = string.Empty;
    public string DriverDocument { get; init; } = string.Empty;
    public DateOnly DriverBirthDate { get; init; }
    public string DriverGender { get; init; } = string.Empty;
    public string DriverMaritalStatus { get; init; } = string.Empty;
    public int DriverLicenseYears { get; init; }
    public string InsuranceType { get; init; } = string.Empty;
    public DateOnly CoverageStartsOn { get; init; }
    public DateOnly CoverageEndsOn { get; init; }
    public DateOnly? PreviousCoverageEndsOn { get; init; }
    public string? PreviousPolicyNumber { get; init; }
    public string? PreviousIdentificationCode { get; init; }
    public int? BonusClass { get; init; }
    public int? ClaimCount { get; init; }
    public IReadOnlyList<RequestedQuoteCoverage> Coverages { get; init; } = new List<RequestedQuoteCoverage>();

    public AutoQuoteSubmission Copy() => this with { Coverages = Coverages.Select(x => x with { }).ToList() };
}

public sealed record RequestedQuoteCoverage
{
    // Normalized catalog code; carrier wire codes belong exclusively to its adapter.
    public string Code { get; init; } = string.Empty;
    public bool Selected { get; init; }
    public decimal? RequestedLimit { get; init; }
    public decimal? RequestedFipePercentage { get; init; }
    public string? RequestedDeductibleType { get; init; }
    public string? RequestedOption { get; init; }
    public int? RequestedQuantity { get; init; }
}
