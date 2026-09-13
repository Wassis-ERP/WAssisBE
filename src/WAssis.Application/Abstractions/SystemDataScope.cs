namespace WAssis.Application.Abstractions;

/// <summary>Explicit capability for audited background processing. Never register in the API host.</summary>
public sealed class SystemDataScope
{
    private SystemDataScope(string purpose) => Purpose = purpose;
    public string Purpose { get; }

    public static SystemDataScope ForWorker(string purpose, Action<string> audit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(purpose);
        ArgumentNullException.ThrowIfNull(audit);
        audit(purpose);
        return new SystemDataScope(purpose);
    }
}
