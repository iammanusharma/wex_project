using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEX.Domain.Exceptions;

namespace WEX.API.Middleware;

/// <summary>
/// Global exception handler that maps domain and validation exceptions
/// to RFC 7807 Problem Details responses. Never exposes stack traces.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, problem) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Extensions =
                    {
                        ["errors"] = ve.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                    }
                }),

            TransactionNotFoundException nfe => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Title = "Transaction not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = nfe.Message
                }),

            CurrencyConversionUnavailableException cce => (
                StatusCodes.Status422UnprocessableEntity,
                new ProblemDetails
                {
                    Title = "Currency conversion unavailable",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = cce.Message
                }),

            HttpRequestException hre => (
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Title = "External service unavailable",
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Detail = "The exchange rate service is temporarily unavailable. Please try again later."
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "An unexpected error occurred",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "Please try again later."
                })
        };

        _logger.LogError(
            exception,
            "Request failed with {StatusCode}: {ExceptionType}",
            statusCode,
            exception.GetType().Name);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
