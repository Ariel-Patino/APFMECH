using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APFMech.WebAPI.Infrastructure;

/// <summary>
/// Global exception handler using ASP.NET Core's built-in diagnostic features.
/// Catch and transform domain and validation exceptions into standard RFC 7807 Problem Details.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Map different exception types to appropriate HTTP status codes and detail objects
        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "One or more validation failures occurred.",
                validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    ) as IDictionary<string, string[]>
            ),
            InvalidOperationException invalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Domain Invariant Violation",
                invalidOperationException.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected system error occurred. Please contact administrative support.",
                null
            )
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // Serialize output response cleanly
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}