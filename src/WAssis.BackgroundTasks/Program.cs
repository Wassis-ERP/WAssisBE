using Serilog;
using WAssis.BackgroundTasks.Modules.Documents;
using WAssis.BackgroundTasks.Modules.Quotes;
using WAssis.BackgroundTasks;
using WAssis.Infra.CrossCutting.IoC;
using WAssis.Application.Abstractions;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddWAssisDockerSecrets();
builder.Services.AddWAssisObservability(builder.Configuration, "WAssis.Worker");

builder.Services.AddSerilog((services, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

NativeInjectorBootStrapper.RegisterServices(builder.Services, builder.Configuration, builder.Environment);
builder.Services.AddScoped(provider => SystemDataScope.ForWorker("BackgroundTasks:durable-queues", purpose =>
    provider.GetRequiredService<ILogger<Worker>>().LogInformation("System data scope opened for {Purpose}", purpose)));
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<QuoteQuoteRequestDispatcherPlaceholder>();
builder.Services.AddHostedService<DocumentSearchDispatcherPlaceholder>();

var host = builder.Build();
if (!builder.Environment.IsDevelopment())
{
    using var scope = host.Services.CreateScope();
    await WAssis.Infra.Data.Configuration.DatabaseMigrationGate.EnsureCurrentAsync(
        scope.ServiceProvider.GetRequiredService<WAssis.Infra.Data.Context.WAssisDbContext>());
}
host.Run();
