using WAssis.Domain.Modules.Quotes.Enums;

namespace WAssis.Services.Api.Modules.Quotes.ViewModels;

public sealed record QuoteResponseViewModel(
    Guid Id,
    string? OfficeBranchId,
    Guid? OpportunityId,
    Guid? InsuranceBranchId,
    Guid? InsuredPersonId,
    string CalculationType,
    string CalculationOrigin,
    string? VersionLabel,
    string CorrelationId,
    string CustomerName,
    string DocumentNumber,
    string? Email,
    string? PhoneNumber,
    string? PostalCode,
    string? CustomerSurname,
    string? CustomerGender,
    string? CustomerMaritalStatusCode,
    DateTime? CustomerBirthDateUtc,
    int? DriverLicenseYears,
    string? DriverLicenseNumber,
    string? InsuredDriverRelationshipCode,
    string? VehicleChassisNumber,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    string? VehicleFipeCode,
    int? VehicleManufactureYear,
    int VehicleModelYear,
    bool? VehicleIsZeroKm,
    bool? VehicleHasTracker,
    bool? VehicleHasAntiTheft,
    bool? VehicleIsFinanced,
    bool? VehicleIsArmored,
    string? VehicleFuelTypeCode,
    string? VehicleOvernightPostalCode,
    bool? VehicleHasKitGas,
    bool HasDriverUnder24,
    bool IsCurrentlyInsured,
    string? PreviousBonus,
    int? BrokerCommissionPercentage,
    int? RenewalInsurerCode,
    QuoteRequestStatus Status,
    string? ShareToken,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<QuoteOptionViewModel> Options);

public sealed record QuoteOptionViewModel(
    Guid Id,
    string InsuranceCompanyCode,
    string InsuranceCompanyName,
    string ProductCode,
    string ProductName,
    QuoteOptionStatus Status,
    decimal? PremiumAmount,
    decimal? CommissionAmount,
    string ExternalReference,
    IReadOnlyCollection<CoverageSnapshotViewModel> Coverages,
    IReadOnlyCollection<InstallmentSnapshotViewModel> Installments,
    IReadOnlyCollection<QuoteStatusMessageViewModel> Messages);

public sealed record CoverageSnapshotViewModel(
    string Code,
    string Name,
    decimal? InsuredAmount,
    decimal? DeductibleAmount);

public sealed record InstallmentSnapshotViewModel(
    int Number,
    decimal Amount,
    decimal? TotalAmount);

public sealed record QuoteStatusMessageViewModel(
    string Code,
    string Description);
