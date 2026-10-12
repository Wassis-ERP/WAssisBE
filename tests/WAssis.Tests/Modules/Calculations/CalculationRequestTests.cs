using WAssis.Application.Modules.Calculations;

namespace WAssis.Tests.Modules.Calculations;

public sealed class CalculationRequestTests
{
    [Fact]
    public void AnnualTermUsesCalendarYearWithLeapDayAdjustment()
    {
        Assert.Equal("2029-02-28", CalculationCalendar.EndOfAnnualTerm("2028-02-29"));
        Assert.Equal("2027-10-12", CalculationCalendar.EndOfAnnualTerm("2026-10-12"));
        Assert.False(CalculationCalendar.OptionalDate("2026-02-29"));
        Assert.False(CalculationCalendar.OptionalDate("12/10/2026"));
    }
    [Fact]
    public void RequestValidationPreservesZeroAndUnknownValues()
    {
        var input = new CreateAutoCalculation { InsuranceBranchId = Guid.NewGuid(), Coverages = [new() { CoverageId = Guid.NewGuid(), RequestedLimit = 0, Selected = true }] };
        var validator = new AutoCalculationInputValidator();
        Assert.True(validator.Validate(input).IsValid);
        Assert.False(validator.Validate(input with { Bonus = 0 }).IsValid); // New policy has no previous bonus.
        Assert.False(validator.Validate(input with { Coverages = [input.Coverages[0], input.Coverages[0]] }).IsValid);
        Assert.False(validator.Validate(input with { Specialization = new() { Form = "VIDA" } }).IsValid);
        Assert.False(validator.Validate(input with { Coverages = null! }).IsValid);
        Assert.False(validator.Validate(input with { SuggestedCommission = -1 }).IsValid);
    }
}
