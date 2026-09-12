using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;
using WAssis.Infra.CrossCutting.IoC;
using WAssis.Infra.Data.Context;
using WAssis.Services.Api.Infrastructure;
using WAssis.Services.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Host.ConfigureHostOptions(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));

builder.Services.AddControllers();
builder.Services.AddConfiguredForwardedHeaders(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Frontend:AllowedOrigins")
            .Get<string[]>() ?? [];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(context.ModelState);
});

NativeInjectorBootStrapper.RegisterServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();
var buildSha = app.Configuration["BUILD_SHA"] ?? "local";
var instance = Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName;
var migrateOnly = args.Contains("--migrate", StringComparer.OrdinalIgnoreCase);

if (migrateOnly)
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    var migrationDbContext = migrationScope.ServiceProvider.GetRequiredService<WAssisDbContext>();
    Log.Information("Applying pending database migrations in one-shot mode");
    await migrationDbContext.Database.MigrateAsync();
    Log.Information("Database migrations are up to date");
    return;
}

if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    if (!app.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "Database:AutoMigrate is restricted to Development. Run this image once with --migrate before deploying replicas.");
    }

    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WAssisDbContext>();
    Log.Information("Applying pending database migrations before the API starts");
    await dbContext.Database.MigrateAsync();
    Log.Information("Database migrations are up to date");
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "WAssis.Services.Api", status = "ok", buildSha, instance })).AllowAnonymous();
app.MapGet("/health/ready", async (WAssisDbContext dbContext, CancellationToken cancellationToken) =>
{
    try
    {
        if (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            return Results.Json(
                new { status = "database_unavailable" },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var pendingMigrations = (await dbContext.Database
            .GetPendingMigrationsAsync(cancellationToken))
            .ToArray();

        return pendingMigrations.Length == 0
            ? Results.Ok(new { status = "ready", buildSha, instance })
            : Results.Json(
                new { status = "migrations_pending", pendingMigrations },
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception exception)
    {
        Log.Warning(exception, "Database readiness check failed");
        return Results.Json(
            new { status = "database_unavailable" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

app.Run();
