using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Npgsql;

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
            PostgresException postgres when postgres.SqlState is
                PostgresErrorCodes.UniqueViolation or
                PostgresErrorCodes.ForeignKeyViolation or
                PostgresErrorCodes.CheckViolation or
                PostgresErrorCodes.ExclusionViolation => (
                StatusCodes.Status409Conflict,
                "Conflito de dados.",
                "O registro conflita com dados existentes ou relacionados."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno.",
                "Nao foi possivel concluir a operacao.")
        };

        var route = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError("API failure {ExceptionType} for route {Route}", exception.GetType().Name, route);
        }
        else
        {
            logger.LogWarning("Rejected API request {ExceptionType} for route {Route}", exception.GetType().Name, route);
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
