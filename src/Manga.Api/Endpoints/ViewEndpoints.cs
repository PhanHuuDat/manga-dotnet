using System.Security.Cryptography;
using System.Text;
using Manga.Application.Common.Interfaces;
using Manga.Application.Views.Commands.TrackView;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Api.Endpoints;

public static class ViewEndpoints
{
    public static void MapViewEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/views").WithTags("Views");

        group.MapPost("track", TrackAsync);
    }

    private static async Task<IResult> TrackAsync(
        TrackViewRequest body, HttpContext httpContext,
        ISender sender, ICurrentUserService currentUser)
    {
        var viewerIdentifier = GetViewerIdentifier(currentUser, httpContext);
        var command = new TrackViewCommand(body.TargetType, body.TargetId, viewerIdentifier);

        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static string GetViewerIdentifier(
        ICurrentUserService currentUser, HttpContext httpContext)
    {
        if (!string.IsNullOrEmpty(currentUser.UserId))
            return currentUser.UserId;

        // Anonymous: SHA256(IP + UserAgent) truncated to 16 hex chars
        // Prefer X-Forwarded-For when behind a reverse proxy
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var raw = $"{ip}:{userAgent}";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..16];
    }
}

internal record TrackViewRequest(ViewTargetType TargetType, Guid TargetId);
