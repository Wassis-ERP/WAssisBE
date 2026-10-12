using WAssis.Application.Modules.Calculations;

namespace WAssis.Tests.Modules.Calculations;

public sealed class PresentationRequestTests
{
    [Fact]
    public void Presentation_RequiresRevisionForUpdateAndSelectionForGeneratedState()
    {
        var validator = new SavePresentationCommandValidator();
        var command = new SavePresentationCommand(Guid.NewGuid(), false, "presentation", new());
        Assert.True(validator.Validate(command).IsValid);
        Assert.False(validator.Validate(command with { Update = true }).IsValid);
        Assert.True(validator.Validate(command with { Update = true, Input = command.Input with { ExpectedRevision = 1 } }).IsValid);
        Assert.False(validator.Validate(command with { Input = command.Input with { Status = "GERADA" } }).IsValid);
    }
    [Fact]
    public void Presentation_RejectsMoreThanFiveDuplicatesAndChoiceOutsideSelection()
    {
        var validator = new SavePresentationCommandValidator();
        var items = Enumerable.Range(0, 6).Select(_ => new PresentationQuoteInput { QuoteId = Guid.NewGuid() }).ToArray();
        var command = new SavePresentationCommand(Guid.NewGuid(), false, "presentation", new() { Items = items });
        Assert.False(validator.Validate(command).IsValid);
        Assert.True(validator.Validate(command with { Input = command.Input with { Items = items.Take(5).ToArray() } }).IsValid);
        Assert.False(validator.Validate(command with { Input = command.Input with { Items = [items[0], items[0]] } }).IsValid);
        Assert.False(validator.Validate(command with { Input = command.Input with { Items = [items[0]], SelectedQuoteId = items[1].QuoteId } }).IsValid);
    }
}
