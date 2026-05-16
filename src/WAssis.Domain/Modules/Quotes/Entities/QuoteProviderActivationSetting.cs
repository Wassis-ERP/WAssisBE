using WAssis.Domain.Core.Entities;

namespace WAssis.Domain.Modules.Quotes.Entities;

public class QuoteProviderActivationSetting : AggregateRoot
{
    public string ProviderCode { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private QuoteProviderActivationSetting()
    {
    }

    private QuoteProviderActivationSetting(Guid id, string providerCode, bool isEnabled)
    {
        Id = id;
        ProviderCode = providerCode;
        IsEnabled = isEnabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static QuoteProviderActivationSetting Create(string providerCode, bool isEnabled)
    {
        return new QuoteProviderActivationSetting(Guid.NewGuid(), providerCode, isEnabled);
    }

    public void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
