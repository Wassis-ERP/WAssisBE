using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WAssis.Services.Api.Extensions;

namespace WAssis.Tests.Infrastructure;

public sealed class ReverseProxyConfigurationTests
{
    [Fact]
    public void AddConfiguredForwardedHeaders_UsesOnlyConfiguredTrustedNetworks()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReverseProxy:ForwardLimit"] = "1",
                ["ReverseProxy:KnownNetworks:0"] = "10.0.0.0/8",
            })
            .Build();
        var services = new ServiceCollection();

        services.AddConfiguredForwardedHeaders(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ForwardedHeadersOptions>>().Value;
        Assert.Equal(1, options.ForwardLimit);
        Assert.Single(options.KnownNetworks);
        Assert.Empty(options.KnownProxies);
        Assert.Equal(
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto,
            options.ForwardedHeaders);
    }

    [Fact]
    public void AddConfiguredForwardedHeaders_RejectsInvalidTrustedNetwork()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReverseProxy:KnownNetworks:0"] = "not-a-network",
            })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddConfiguredForwardedHeaders(configuration));

        Assert.Contains("valid CIDR", exception.Message, StringComparison.Ordinal);
    }
}
