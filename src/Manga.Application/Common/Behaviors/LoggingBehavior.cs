using MediatR;
using Microsoft.Extensions.Logging;

namespace Manga.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request/response timing.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var response = await next(ct);

        logger.LogInformation("Handled {RequestName}", requestName);
        return response;
    }
}
