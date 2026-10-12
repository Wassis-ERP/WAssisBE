using System.Globalization;
using FluentValidation;

namespace WAssis.Application.Modules.Calculations;

public sealed class CreateCalculationCommandValidator : AbstractValidator<CreateCalculationCommand>
{
    public CreateCalculationCommandValidator()
    {
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(64).Matches("^[A-Za-z0-9_-]+$");
        RuleFor(x => x.Input).NotNull().SetValidator(new AutoCalculationInputValidator());
        RuleFor(x => x).Must(x => x.Input is null || x.Input.OpportunityId == Guid.Empty || x.Input.OpportunityId == x.OpportunityId)
            .WithMessage("A oportunidade do pedido deve corresponder à rota.");
    }
}

public sealed class AutoCalculationInputValidator : AbstractValidator<CreateAutoCalculation>
{
    public AutoCalculationInputValidator()
    {
        RuleFor(x => x.InsuranceBranchId).NotEmpty();
        RuleFor(x => x.InsuredPersonId).NotEqual(Guid.Empty);
        RuleFor(x => x.PreviousInsurerId).NotEqual(Guid.Empty);
        RuleFor(x => x.Origin).Must(x => x is "MANUAL" or "ASSISTIDA" or "IMPORTADA");
        RuleFor(x => x.InsuranceType).Must(x => x is "NOVO" or "RENOVACAO_PROPRIA" or "RENOVACAO_OUTRA");
        RuleFor(x => x.Bonus).InclusiveBetween(0, 10);
        RuleFor(x => x.ClaimCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PartialLossClaimCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SuggestedCommission).InclusiveBetween(0, 100);
        RuleFor(x => x.CoverageStartsOn).Must(CalculationCalendar.OptionalDate);
        RuleFor(x => x.CoverageEndsOn).Must(CalculationCalendar.OptionalDate);
        RuleFor(x => x.PreviousCoverageEndsOn).Must(CalculationCalendar.OptionalDate);
        RuleFor(x => x.PreviousPolicyNumber).MaximumLength(120);
        RuleFor(x => x.PreviousIdentification).MaximumLength(120);
        RuleFor(x => x.PreviousPolicyStatus).MaximumLength(80);
        RuleFor(x => x.InternalNote).MaximumLength(2000);
        When(x => x.InsuranceType == "NOVO", () =>
        {
            RuleFor(x => x.PreviousInsurerId).Null();
            RuleFor(x => x.Bonus).Null();
            RuleFor(x => x.PreviousCoverageEndsOn).Null();
            RuleFor(x => x.PreviousPolicyNumber).Empty();
            RuleFor(x => x.PreviousIdentification).Empty();
            RuleFor(x => x.PreviousPolicyStatus).Empty();
        });
        When(x => x.InsuranceType == "RENOVACAO_OUTRA", () =>
        {
            RuleFor(x => x.PreviousInsurerId).NotNull();
            RuleFor(x => x.PreviousPolicyNumber).NotEmpty();
            RuleFor(x => x.PreviousCoverageEndsOn).NotEmpty();
            RuleFor(x => x.CoverageStartsOn).NotEmpty();
        });
        RuleFor(x => x.Specialization).NotNull();
        When(x => x.Specialization is not null, () =>
        {
            RuleFor(x => x.Specialization.Form).Equal("AUTO");
            RuleFor(x => x.Specialization.Data).NotNull().SetValidator(new AutoRiskValidator());
        });
        RuleFor(x => x.Coverages).NotNull().Must(x => x is { Count: <= 100 });
        RuleFor(x => x.Coverages).Must(x => x is null || x.All(c => c is not null) && x.Select(c => c.CoverageId).Distinct().Count() == x.Count);
        RuleForEach(x => x.Coverages).NotNull().SetValidator(new CalculationCoverageValidator());
    }
}

public sealed class AutoRiskValidator : AbstractValidator<AutoCalculationRisk>
{
    public AutoRiskValidator()
    {
        RuleFor(x => x.ManufactureYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.ModelYear).InclusiveBetween(1900, 2100);
        RuleFor(x => x.MonthlyKilometres).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DriverLicenseYears).InclusiveBetween(0, 80);
        RuleFor(x => x.DriverBirthDate).Must(CalculationCalendar.OptionalDate);
        RuleFor(x => x.DriverGender).Must(x => x is null or "M" or "F");
        RuleFor(x => x).Must(x => typeof(AutoCalculationRisk).GetProperties()
            .Where(p => p.PropertyType == typeof(string)).All(p => p.GetValue(x) is not string value || value.Length <= 200))
            .WithMessage("Os campos de texto do risco Auto aceitam no máximo 200 caracteres.");
    }
}
public sealed class CalculationCoverageValidator : AbstractValidator<RequestedCalculationCoverage>
{
    public CalculationCoverageValidator()
    {
        RuleFor(x => x.CoverageId).NotEmpty();
        RuleFor(x => x.RequestedLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedFipePercentage).InclusiveBetween(0, 200);
        RuleFor(x => x.RequestedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedDeductibleType).MaximumLength(80);
        RuleFor(x => x.RequestedOption).MaximumLength(200);
        RuleFor(x => x.TransmittedObservation).MaximumLength(2000);
    }
}

public static class CalculationCalendar
{
    public static bool OptionalDate(string? value) => value is null || DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    public static string EndOfAnnualTerm(string start) => DateOnly.ParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddYears(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
