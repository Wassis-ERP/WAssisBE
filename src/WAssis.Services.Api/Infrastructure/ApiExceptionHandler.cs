using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WAssis.Services.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Acesso negado.",
                exception.Message),
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Dados invalidos.",
                "Revise os campos enviados e tente novamente."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno.",
                "Nao foi possivel concluir a operacao.")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled API exception for {Path}", httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Rejected API request for {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            },
            Exception = exception
        });
    }
}
