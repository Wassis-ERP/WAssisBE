using Microsoft.Extensions.Options;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Bradesco.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Icatu;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Auto;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Business;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Life;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Residence;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Liberty.Travel;

namespace WAssis.Tests.Modules.Quotes;

public sealed class QuoteProviderDescriptorTests
{
    [Fact]
    public void BradescoProvider_Describe_ShouldExposeMissingRequirements()
    {
        var provider = new BradescoAutoQuoteProvider(Options.Create(new BradescoAutoQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.True(descriptor.IsEnabled);
        Assert.False(descriptor.IsReady);
        Assert.Equal("bradesco_auto", descriptor.ProviderCode);
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

    [Fact]
    public void LibertyLifeProvider_Describe_ShouldExposeLifeModuleRequirements()
    {
        var provider = new LibertyLifeQuoteProvider(Options.Create(new LibertyLifeQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.True(descriptor.IsEnabled);
        Assert.False(descriptor.IsReady);
        Assert.Equal("liberty_vida", descriptor.ProviderCode);
        Assert.Equal("vida", descriptor.ProductLine);
        Assert.Contains(descriptor.Requirements, static item => item.Code == "commercial_product_code" && !item.IsConfigured);
    }

    [Fact]
    public void LibertyAutoProvider_Describe_ShouldDefaultToAutoProductLine()
    {
        var provider = new LibertyAutoQuoteProvider(Options.Create(new LibertyAutoQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.Equal("liberty_auto", descriptor.ProviderCode);
        Assert.Equal("auto", descriptor.ProductLine);
    }

    [Fact]
    public void LibertyResidenceProvider_Describe_ShouldDefaultToResidenceProductLine()
    {
        var provider = new LibertyResidenceQuoteProvider(Options.Create(new LibertyResidenceQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.Equal("liberty_residence", descriptor.ProviderCode);
        Assert.Equal("residencial", descriptor.ProductLine);
    }

    [Fact]
    public void LibertyBusinessProvider_Describe_ShouldDefaultToBusinessProductLine()
    {
        var provider = new LibertyBusinessQuoteProvider(Options.Create(new LibertyBusinessQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.Equal("liberty_business", descriptor.ProviderCode);
        Assert.Equal("empresarial", descriptor.ProductLine);
    }

    [Fact]
    public void LibertyTravelProvider_Describe_ShouldDefaultToTravelProductLine()
    {
        var provider = new LibertyTravelQuoteProvider(Options.Create(new LibertyTravelQuoteOptions
        {
            Enabled = true
        }));

        var descriptor = provider.Describe();

        Assert.Equal("liberty_travel", descriptor.ProviderCode);
        Assert.Equal("seguro_viagem", descriptor.ProductLine);
    }
}
