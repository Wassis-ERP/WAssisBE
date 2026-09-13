using System.Diagnostics;
using Serilog.Context;

namespace WAssis.Services.Api.Infrastructure;

public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var value = context.Request.Headers["X-Correlation-ID"].ToString();
        var correlationId = Guid.TryParse(value, out var parsed) ? parsed.ToString() : Guid.NewGuid().ToString();
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        Activity.Current?.SetTag("wassis.correlation_id", correlationId);
        using (LogContext.PushProperty("CorrelationId", correlationId)) await next(context);
    }
}
