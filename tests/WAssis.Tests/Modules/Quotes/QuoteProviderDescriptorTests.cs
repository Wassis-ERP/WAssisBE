using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Bradesco;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Icatu;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteProviderDescriptorTests
{
    [Fact]
    public void BradescoProvider_Describe_ShouldExposeMissingRequirements()
    {
        var provider = new BradescoQuoteProvider(Options.Create(new BradescoQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.True(descriptor.IsEnabled);
        Assert.False(descriptor.IsReady);
        Assert.Equal("bradesco_seguros", descriptor.ProviderCode);
        Assert.Contains(descriptor.Requirements, static item => item.Code == "mutual_tls_certificate" && !item.IsConfigured);
    }

    [Fact]
    public void IcatuProvider_Describe_ShouldBeReady_WhenPartnerFieldsAreConfigured()
    {
        var provider = new IcatuQuoteProvider(Options.Create(new IcatuQuoteOptions
        {
            Enabled = true,
            ProductLine = "vida",
            ApiCatalogKey = "catalogo-vida",
            ClientId = "client-id",
            ClientSecret = "client-secret",
            PartnerId = "partner-01",
            ApplicationId = "app-01",
            CertificateId = "cert-01"
        }));

        var descriptor = provider.Describe();

        Assert.True(descriptor.IsEnabled);
        Assert.True(descriptor.IsReady);
        Assert.Equal("icatu_seguros", descriptor.ProviderCode);
        Assert.All(descriptor.Requirements, static item => Assert.True(item.IsConfigured));
    }
}
