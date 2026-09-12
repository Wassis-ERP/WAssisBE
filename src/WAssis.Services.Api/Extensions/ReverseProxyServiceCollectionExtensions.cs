using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using WAssis.Services.Api.Configuration;

namespace WAssis.Services.Api.Extensions;

public static class ReverseProxyServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguredForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var proxyOptions = configuration
            .GetSection(ReverseProxyOptions.SectionName)
            .Get<ReverseProxyOptions>() ?? new ReverseProxyOptions();

        if (proxyOptions.ForwardLimit <= 0)
        {
            throw new InvalidOperationException("ReverseProxy:ForwardLimit must be greater than zero.");
        }

        var knownNetworks = proxyOptions.KnownNetworks.Select(ParseNetwork).ToArray();
        if (knownNetworks.Length == 0)
        {
            throw new InvalidOperationException("ReverseProxy:KnownNetworks must contain at least one trusted CIDR.");
        }

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedHost |
                                       ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = proxyOptions.ForwardLimit;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            foreach (var network in knownNetworks)
            {
                options.KnownNetworks.Add(network);
            }
        });

        return services;
    }

    internal static Microsoft.AspNetCore.HttpOverrides.IPNetwork ParseNetwork(string value)
    {
        var parts = value.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !IPAddress.TryParse(parts[0], out var prefix) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength))
        {
            throw new InvalidOperationException($"Reverse proxy network '{value}' is not a valid CIDR.");
        }

        var maximumPrefixLength = prefix.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
        if (prefixLength < 0 || prefixLength > maximumPrefixLength)
        {
            throw new InvalidOperationException($"Reverse proxy network '{value}' has an invalid prefix length.");
        }

        return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
    }
}
