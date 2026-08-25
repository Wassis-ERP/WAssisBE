using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WAssis.Application.DependencyInjection;
using WAssis.Application.Modules.Quotes.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Extensions;
using WAssis.Application.Modules.Quotes.Services;
using WAssis.Infra.Data.DependencyInjection;

namespace WAssis.Infra.CrossCutting.IoC;

public static class NativeInjectorBootStrapper
{
    public static void RegisterServices(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.AddWAssisIdentity(configuration);
        services.AddApplicationServices();
        services.AddInfraDataServices(configuration, hostEnvironment);
        services.AddScoped<IQuoteProviderRegistry, QuoteProviderRegistry>();
    }
}
