using Serilog;
using WAssis.BackgroundTasks.Modules.Documents;
using WAssis.BackgroundTasks.Modules.Quotes;
using WAssis.BackgroundTasks;
using WAssis.Infra.CrossCutting.IoC;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
    configuration.ReadFrom.Configuration(builder.Configuration));

NativeInjectorBootStrapper.RegisterServices(builder.Services, builder.Configuration);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<QuoteQuoteRequestDispatcherPlaceholder>();
builder.Services.AddHostedService<DocumentSearchDispatcherPlaceholder>();

var host = builder.Build();
host.Run();
