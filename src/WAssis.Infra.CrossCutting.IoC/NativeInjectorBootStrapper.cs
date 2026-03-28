using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.DependencyInjection;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Extensions;
using WAssis.Application.Modules.Quotes.Services;
using WAssis.Infra.Data.DependencyInjection;

namespace WAssis.Infra.CrossCutting.IoC;

public static class NativeInjectorBootStrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddWAssisIdentity(configuration);
        services.AddApplicationServices();
        services.AddInfraDataServices(configuration);
        services.AddScoped<IQuoteProviderRegistry, QuoteProviderRegistry>();
    }
}
