using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using WAssis.Domain.Modules.Quotes;
using WAssis.Application.Modules.Calculations;

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
            CalculationConflictException => (
                StatusCodes.Status409Conflict,
                "Chave de idempotência em conflito.",
                "Use uma nova chave para uma nova versão do cálculo."),
            QuoteRequestConflictException => (
                StatusCodes.Status409Conflict,
                "Chave de idempotência em conflito.",
                "Use uma nova chave para um pedido diferente."),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Acesso negado.",
                exception.Message),
            ValidationException => (
                StatusCodes.Status400BadRequest,
                "Dados invalidos.",
                "Revise os campos enviados e tente novamente."),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "O registro foi alterado.",
                "Atualize os dados antes de tentar novamente."),
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
