using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Application.Modules.Quotes;

internal static class QuoteRequestMappings
{
    public static QuoteRequestDto ToDto(QuoteRequest quoteRequest)
    {
        return new QuoteRequestDto(
            quoteRequest.Id,
            quoteRequest.OfficeBranchId,
            quoteRequest.OpportunityId,
            quoteRequest.InsuranceBranchId,
            quoteRequest.InsuredPersonId,
            quoteRequest.CalculationType,
            quoteRequest.CalculationOrigin,
            quoteRequest.VersionLabel,
            quoteRequest.CorrelationId,
            quoteRequest.CustomerName,
            quoteRequest.DocumentNumber,
            quoteRequest.Email,
            quoteRequest.PhoneNumber,
            quoteRequest.PostalCode,
            quoteRequest.CustomerSurname,
            quoteRequest.CustomerGender,
            quoteRequest.CustomerMaritalStatusCode,
            quoteRequest.CustomerBirthDateUtc,
            quoteRequest.DriverLicenseYears,
            quoteRequest.DriverLicenseNumber,
            quoteRequest.InsuredDriverRelationshipCode,
            quoteRequest.VehicleChassisNumber,
            quoteRequest.VehiclePlate,
            quoteRequest.VehicleBrand,
            quoteRequest.VehicleModel,
            quoteRequest.VehicleFipeCode,
            quoteRequest.VehicleManufactureYear,
            quoteRequest.VehicleModelYear,
            quoteRequest.VehicleIsZeroKm,
            quoteRequest.VehicleHasTracker,
            quoteRequest.VehicleHasAntiTheft,
            quoteRequest.VehicleIsFinanced,
            quoteRequest.VehicleIsArmored,
            quoteRequest.VehicleFuelTypeCode,
            quoteRequest.VehicleOvernightPostalCode,
            quoteRequest.VehicleHasKitGas,
            quoteRequest.HasDriverUnder24,
            quoteRequest.IsCurrentlyInsured,
            quoteRequest.PreviousBonus,
            quoteRequest.BrokerCommissionPercentage,
            quoteRequest.RenewalInsurerCode,
            quoteRequest.Status,
            quoteRequest.ShareToken,
            quoteRequest.CreatedAtUtc,
            quoteRequest.UpdatedAtUtc,
            quoteRequest.Options.Select(static option => new QuoteOptionDto(
                option.Id,
                option.InsuranceCompanyCode,
                option.InsuranceCompanyName,
                option.ProductCode,
                option.ProductName,
                option.Status,
                option.PremiumAmount,
                option.CommissionAmount,
                option.ExternalReference,
                option.Coverages.Select(static coverage => new CoverageSnapshotDto(
                    coverage.Code,
                    coverage.Name,
                    coverage.InsuredAmount,
                    coverage.DeductibleAmount)).ToArray(),
                option.Installments.Select(static installment => new InstallmentSnapshotDto(
                    installment.Number,
                    installment.Amount,
                    installment.TotalAmount)).ToArray(),
                option.Messages.Select(static message => new QuoteStatusMessageDto(
                    message.Code,
                    message.Description)).ToArray())).ToArray());
    }
}
