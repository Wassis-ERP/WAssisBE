using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace WAssis.Services.Api.Infrastructure;

public sealed class CsrfProtectionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        var unsafeMethod = !HttpMethods.IsGet(context.Request.Method)
            && !HttpMethods.IsHead(context.Request.Method)
            && !HttpMethods.IsOptions(context.Request.Method)
            && !HttpMethods.IsTrace(context.Request.Method);
        if (unsafeMethod && context.User.Identity?.IsAuthenticated == true
            && context.Request.Cookies.ContainsKey("wassis_access"))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Requisição inválida.",
                    Detail = "Token CSRF ausente ou inválido.",
                    Extensions = { ["code"] = "csrf_invalid" },
                });
                return;
            }
        }
        await next(context);
    }
}
