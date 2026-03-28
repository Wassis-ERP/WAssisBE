using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WAssis.Application.DependencyInjection;
using WAssis.Infra.Data.DependencyInjection;

namespace WAssis.Infra.CrossCutting.IoC;

public static class NativeInjectorBootStrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationServices();
        services.AddInfraDataServices(configuration);
    }
}
