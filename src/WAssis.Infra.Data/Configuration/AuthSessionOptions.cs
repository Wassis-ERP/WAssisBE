namespace WAssis.Infra.Data.Configuration;

public sealed class AuthSessionOptions
{
    public const string SectionName = "Identity:Session";
    public int IdleTimeoutMinutes { get; set; } = 30;
}
