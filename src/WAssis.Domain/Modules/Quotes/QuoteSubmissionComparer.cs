using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.ValueObjects;

namespace WAssis.Domain.Modules.Quotes;

/// <summary>Compares the persisted input, excluding processing state and generated values.</summary>
public static class QuoteSubmissionComparer
{
    public static bool Matches(QuoteRequest left, QuoteRequest right) =>
        left.TenantId == right.TenantId && left.OfficeBranchId == right.OfficeBranchId &&
        left.CorrelationId == right.CorrelationId && left.OpportunityId == right.OpportunityId &&
        left.InsuranceBranchId == right.InsuranceBranchId && left.InsuredPersonId == right.InsuredPersonId &&
        left.CalculationType == right.CalculationType && left.CalculationOrigin == right.CalculationOrigin &&
        left.VersionLabel == right.VersionLabel && left.CustomerName == right.CustomerName &&
        left.DocumentNumber == right.DocumentNumber && left.Email == right.Email &&
        left.PhoneNumber == right.PhoneNumber && left.PostalCode == right.PostalCode &&
        left.CustomerSurname == right.CustomerSurname && left.CustomerGender == right.CustomerGender &&
        left.CustomerMaritalStatusCode == right.CustomerMaritalStatusCode &&
        Microseconds(left.CustomerBirthDateUtc) == Microseconds(right.CustomerBirthDateUtc) &&
        left.DriverLicenseYears == right.DriverLicenseYears && left.DriverLicenseNumber == right.DriverLicenseNumber &&
        left.InsuredDriverRelationshipCode == right.InsuredDriverRelationshipCode &&
        left.VehicleChassisNumber == right.VehicleChassisNumber && left.VehiclePlate == right.VehiclePlate &&
        left.VehicleBrand == right.VehicleBrand && left.VehicleModel == right.VehicleModel &&
        left.VehicleFipeCode == right.VehicleFipeCode && left.VehicleManufactureYear == right.VehicleManufactureYear &&
        left.VehicleModelYear == right.VehicleModelYear && left.VehicleIsZeroKm == right.VehicleIsZeroKm &&
        left.VehicleHasTracker == right.VehicleHasTracker && left.VehicleHasAntiTheft == right.VehicleHasAntiTheft &&
        left.VehicleIsFinanced == right.VehicleIsFinanced && left.VehicleIsArmored == right.VehicleIsArmored &&
        left.VehicleFuelTypeCode == right.VehicleFuelTypeCode && left.VehicleOvernightPostalCode == right.VehicleOvernightPostalCode &&
        left.VehicleHasKitGas == right.VehicleHasKitGas && left.HasDriverUnder24 == right.HasDriverUnder24 &&
        left.IsCurrentlyInsured == right.IsCurrentlyInsured && left.PreviousBonus == right.PreviousBonus &&
        left.BrokerCommissionPercentage == right.BrokerCommissionPercentage && left.RenewalInsurerCode == right.RenewalInsurerCode &&
        Matches(left.AutoSubmission, right.AutoSubmission);

    private static long? Microseconds(DateTime? value) => value?.Ticks / 10;

    private static bool Matches(AutoQuoteSubmission? left, AutoQuoteSubmission? right)
    {
        if (left is null || right is null) return left is null && right is null;
        // Collection order is not business data and may change on an EF roundtrip.
        return (left with { Coverages = Array.Empty<RequestedQuoteCoverage>() }) ==
               (right with { Coverages = Array.Empty<RequestedQuoteCoverage>() }) &&
               left.Coverages.OrderBy(x => x.Code, StringComparer.Ordinal)
                   .SequenceEqual(right.Coverages.OrderBy(x => x.Code, StringComparer.Ordinal));
    }
}
