namespace WAssis.Domain.Modules.Quotes.ValueObjects;

public class QuoteStatusMessage
{
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private QuoteStatusMessage()
    {
    }

    public QuoteStatusMessage(string code, string description)
    {
        Code = code;
        Description = description;
    }
}
