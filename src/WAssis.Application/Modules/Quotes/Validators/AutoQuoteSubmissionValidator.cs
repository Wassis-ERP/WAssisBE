using FluentValidation;
using WAssis.Domain.Modules.Quotes.ValueObjects;

namespace WAssis.Application.Modules.Quotes.Validators;

public sealed class AutoQuoteSubmissionValidator : AbstractValidator<AutoQuoteSubmission>
{
    public AutoQuoteSubmissionValidator()
    {
        RuleFor(x => x.DriverName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DriverDocument).NotEmpty().MaximumLength(32);
        RuleFor(x => x.DriverBirthDate).NotEqual(default(DateOnly))
            .LessThan(x => x.CoverageStartsOn);
        RuleFor(x => x.DriverGender).Must(x => x is "M" or "F");
        RuleFor(x => x.DriverMaritalStatus).NotEmpty().MaximumLength(64);
        RuleFor(x => x.DriverLicenseYears).InclusiveBetween(0, 80);
        RuleFor(x => x.InsuranceType).Must(x => x is "NOVO" or "RENOVACAO_PROPRIA" or "RENOVACAO_OUTRA");
        RuleFor(x => x.CoverageStartsOn).NotEqual(default(DateOnly));
        RuleFor(x => x.CoverageEndsOn).GreaterThan(x => x.CoverageStartsOn);
        RuleFor(x => x.BonusClass).InclusiveBetween(0, 10);
        RuleFor(x => x.ClaimCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PreviousPolicyNumber).MaximumLength(120);
        RuleFor(x => x.PreviousIdentificationCode).MaximumLength(120);
        When(x => x.InsuranceType == "NOVO", () =>
        {
            RuleFor(x => x.PreviousCoverageEndsOn).Null();
            RuleFor(x => x.PreviousPolicyNumber).Empty();
            RuleFor(x => x.PreviousIdentificationCode).Empty();
            RuleFor(x => x.BonusClass).Null();
            RuleFor(x => x.ClaimCount).Null();
        });
        When(x => x.InsuranceType is "RENOVACAO_PROPRIA" or "RENOVACAO_OUTRA", () =>
        {
            RuleFor(x => x.PreviousCoverageEndsOn).NotNull();
            RuleFor(x => x.PreviousPolicyNumber).NotEmpty();
        });
        When(x => x.InsuranceType == "RENOVACAO_PROPRIA", () =>
            RuleFor(x => x.CoverageStartsOn).Must((x, start) => start == x.PreviousCoverageEndsOn)
                .WithMessage("A renovação própria deve preservar a continuidade da vigência."));
        RuleFor(x => x.Coverages).NotNull().Must(x => x is { Count: > 0 and <= 100 })
            .WithMessage("Informe de 1 a 100 preferências de cobertura.");
        RuleFor(x => x.Coverages).Must(x => x is null ||
            x.All(c => c is not null) && x.Select(c => c.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() == x.Count)
            .WithMessage("Coberturas devem ser únicas e não nulas.");
        RuleForEach(x => x.Coverages).NotNull().SetValidator(new RequestedQuoteCoverageValidator());
    }
}

public sealed class RequestedQuoteCoverageValidator : AbstractValidator<RequestedQuoteCoverage>
{
    public RequestedQuoteCoverageValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(80).Matches("^[a-z0-9]+(-[a-z0-9]+)*$");
        RuleFor(x => x.RequestedLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedFipePercentage).InclusiveBetween(0, 200);
        RuleFor(x => x.RequestedDeductibleType).MaximumLength(80);
        RuleFor(x => x.RequestedOption).MaximumLength(80);
        RuleFor(x => x.RequestedQuantity).GreaterThanOrEqualTo(0);
    }
}
