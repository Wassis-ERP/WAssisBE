using FluentValidation;

namespace WAssis.Application.Modules.Calculations;

public static class ExecutionValidation
{
    public static bool ValidKey(string? value) => value is { Length: > 0 and <= 64 } && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-');
    public static bool ValidReference(string? value) => value is { Length: > 0 and <= 100 } && value.Trim() == value && !value.Any(char.IsControl);
    public static bool ValidLink(string? value) => value is null || value.Length <= 2000 && Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo);
    public static bool NonNegativeNumbers<T>(T value) where T : notnull => typeof(T).GetProperties()
        .All(p => p.GetValue(value) switch { decimal d => d >= 0 && d <= 1_000_000_000_000m, int n => n >= 0, _ => true });
    public static bool BoundedText<T>(T value) where T : notnull => typeof(T).GetProperties()
        .All(p => p.GetValue(value) is not string s || s.Length <= 4000);
}
public sealed class CreateExecutionCommandValidator : AbstractValidator<CreateExecutionCommand>
{
    public CreateExecutionCommandValidator()
    {
        RuleFor(x => x.CalculationId).NotEmpty(); RuleFor(x => x.IdempotencyKey).Must(ExecutionValidation.ValidKey);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.InsurerId).NotEmpty(); RuleFor(x => x.Input.Engine).Equal("MANUAL");
            RuleFor(x => x.Input.CommissionOverride).InclusiveBetween(0, 100);
        });
    }
}
public sealed class RetryExecutionCommandValidator : AbstractValidator<RetryExecutionCommand>
{
    public RetryExecutionCommandValidator()
    {
        RuleFor(x => x.ExecutionId).NotEmpty(); RuleFor(x => x.IdempotencyKey).Must(ExecutionValidation.ValidKey);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () => RuleFor(x => x.Input.CommissionOverride).InclusiveBetween(0, 100));
    }
}
public sealed class CancelExecutionCommandValidator : AbstractValidator<CancelExecutionCommand>
{
    public CancelExecutionCommandValidator() { RuleFor(x => x.ExecutionId).NotEmpty(); RuleFor(x => x.IdempotencyKey).Must(ExecutionValidation.ValidKey); }
}
public sealed class RecordManualQuoteCommandValidator : AbstractValidator<RecordManualQuoteCommand>
{
    public RecordManualQuoteCommandValidator()
    {
        RuleFor(x => x.ExecutionId).NotEmpty(); RuleFor(x => x.IdempotencyKey).Must(ExecutionValidation.ValidKey);
        RuleFor(x => x.Input).NotNull().SetValidator(new ManualQuoteValidator());
    }
}
public sealed class ManualQuoteValidator : AbstractValidator<RecordManualQuote>
{
    public ManualQuoteValidator()
    {
        RuleFor(x => x.TotalPremium).NotNull();
        RuleFor(x => x).Must(ExecutionValidation.NonNegativeNumbers).Must(ExecutionValidation.BoundedText);
        RuleFor(x => x.ValidUntil).Must(CalculationCalendar.OptionalDate);
        RuleFor(x => x.ProposalLink).Must(ExecutionValidation.ValidLink);
        RuleFor(x => x.InsurerQuoteNumber).MaximumLength(200);
        RuleFor(x => x.Coverages).NotNull().Must(x => x is { Count: <= 100 });
        RuleFor(x => x.Coverages).Must(x => x is null || x.All(c => c is not null) && x.Select(c => c.ResultKey).Distinct(StringComparer.Ordinal).Count() == x.Count);
        RuleForEach(x => x.Coverages).NotNull().SetValidator(new ReturnedQuoteCoverageValidator());
        RuleFor(x => x.Installments).NotNull().Must(x => x is { Count: <= 100 });
        RuleFor(x => x.Installments).Must(x => x is null || x.All(c => c is not null) && x.Select(c => c.OptionCode).Distinct(StringComparer.Ordinal).Count() == x.Count && x.Count(c => c.Primary == true) <= 1);
        RuleForEach(x => x.Installments).NotNull().SetValidator(new QuoteInstallmentValidator());
    }
}
public sealed class ReturnedQuoteCoverageValidator : AbstractValidator<ReturnedQuoteCoverage>
{
    public ReturnedQuoteCoverageValidator()
    {
        RuleFor(x => x.ResultKey).Must(ExecutionValidation.ValidReference);
        RuleFor(x => x.CoverageId).Must(x => x is null || x != Guid.Empty);
        RuleFor(x => x).Must(ExecutionValidation.NonNegativeNumbers).Must(ExecutionValidation.BoundedText);
        RuleFor(x => x.AcceptedFipePercentage).InclusiveBetween(0, 200);
        RuleFor(x => x.MandatoryParticipation).InclusiveBetween(0, 100);
    }
}
public sealed class QuoteInstallmentValidator : AbstractValidator<QuoteInstallmentOption>
{
    public QuoteInstallmentValidator()
    {
        RuleFor(x => x.OptionCode).Must(ExecutionValidation.ValidReference);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InstallmentCount).InclusiveBetween(1, 360);
        RuleFor(x => x).Must(ExecutionValidation.NonNegativeNumbers);
        RuleFor(x => x.InterestPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.FirstDueDate).Must(CalculationCalendar.OptionalDate);
    }
}
