using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WalletLedger.Application.Exceptions;
using WalletLedger.Domain;

namespace WalletLedger.Api.Errors;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = Describe(exception);

        if (problem.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });
    }

    private static ProblemDetails Describe(Exception exception)
    {
        var problem = exception switch
        {
            AuthenticationFailedException => Create(StatusCodes.Status401Unauthorized, "Authentication failed."),
            ConflictException conflict => Create(StatusCodes.Status409Conflict, "Conflict.", conflict.Message),
            RequestValidationException => Create(StatusCodes.Status400BadRequest, "Validation failed."),
            DomainException domain => Create(StatusCodes.Status422UnprocessableEntity, "Business rule violated.", domain.Message),
            _ => Create(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (exception is RequestValidationException validation)
        {
            problem.Extensions["errors"] = validation.Errors;
        }

        return problem;
    }

    private static ProblemDetails Create(int status, string title, string? detail = null) =>
        new() { Status = status, Title = title, Detail = detail };
}