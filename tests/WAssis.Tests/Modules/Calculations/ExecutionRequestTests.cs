using WAssis.Application.Modules.Calculations;

namespace WAssis.Tests.Modules.Calculations;

public sealed class ExecutionRequestTests
{
    [Fact]
    public void ManualExecution_ValidatesEngineCommissionAndKey()
    {
        var validator = new CreateExecutionCommandValidator();
        var command = new CreateExecutionCommand(Guid.NewGuid(), "manual-1", new() { InsurerId = Guid.NewGuid(), CommissionOverride = 0 });
        Assert.True(validator.Validate(command).IsValid);
        Assert.False(validator.Validate(command with { Input = command.Input with { Engine = "AGGER" } }).IsValid);
        Assert.False(validator.Validate(command with { Input = command.Input with { CommissionOverride = 101 } }).IsValid);
        Assert.False(validator.Validate(command with { IdempotencyKey = " " }).IsValid);
    }
    [Fact]
    public void ManualQuote_PreservesZeroAndUnknown_RejectsUnsafeLinksAndDates()
    {
        var validator = new ManualQuoteValidator();
        var input = new RecordManualQuote { TotalPremium = 0, NetPremium = null, ValidUntil = "2028-02-29" };
        Assert.True(validator.Validate(input).IsValid);
        Assert.False(validator.Validate(input with { TotalPremium = null }).IsValid);
        Assert.False(validator.Validate(input with { Tax = -1 }).IsValid);
        Assert.False(validator.Validate(input with { ProposalLink = "javascript:alert(1)" }).IsValid);
        Assert.False(validator.Validate(input with { ProposalLink = "https://user:password@example.test" }).IsValid);
        Assert.False(validator.Validate(input with { ValidUntil = "2027-02-29" }).IsValid);
    }
    [Fact]
    public void ManualQuote_RejectsDuplicateCoverageKeysAndMultiplePrimaryOptions()
    {
        var validator = new ManualQuoteValidator();
        var input = new RecordManualQuote { TotalPremium = 10, Coverages = [new() { ResultKey = "casco" }, new() { ResultKey = "casco" }] };
        Assert.False(validator.Validate(input).IsValid);
        input = input with { Coverages = [], Installments = [new() { OptionCode = "a", PaymentMethod = "BOLETO", InstallmentCount = 1, Primary = true }, new() { OptionCode = "b", PaymentMethod = "CARTAO", InstallmentCount = 10, Primary = true }] };
        Assert.False(validator.Validate(input).IsValid);
        Assert.True(validator.Validate(input with { Installments = [input.Installments[0], input.Installments[1] with { Primary = null }] }).IsValid);
    }
}
