namespace WAssis.Services.Api.Configuration;

public sealed class ReverseProxyOptions
{
    public const string SectionName = "ReverseProxy";

    public int ForwardLimit { get; init; } = 2;
    public string[] KnownNetworks { get; init; } = [];
}
