using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WAssis.Application.Abstractions;

namespace WAssis.Services.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class DistributedOperationLimitAttribute(
    string operation,
    int permitLimit,
    int windowSeconds) : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var limiter = context.HttpContext.RequestServices.GetRequiredService<IDistributedOperationLimiter>();
        var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserContext>();
        var decision = await limiter.AcquireAsync(
            operation,
            user.TenantId,
            user.UserId,
            context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            permitLimit,
            TimeSpan.FromSeconds(windowSeconds),
            context.HttpContext.RequestAborted);
        if (!decision.Allowed)
        {
            var retryAfter = Math.Max(1, (int)Math.Ceiling((decision.ResetAt - DateTimeOffset.UtcNow).TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Limite de operação excedido.",
                Detail = "Aguarde antes de repetir esta operação.",
                Extensions = { ["code"] = "operation_rate_limited" },
            }) { StatusCode = StatusCodes.Status429TooManyRequests };
            return;
        }
        await next();
    }
}
