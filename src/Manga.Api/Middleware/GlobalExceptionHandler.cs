using FluentValidation;
using Manga.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Middleware;

/// <summary>
/// Global exception handler mapping domain/validation exceptions to ProblemDetails.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken ct)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundEx.Message),

            DomainException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Domain Error",
                domainEx.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server Error",
                "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception: {Title}", title);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
