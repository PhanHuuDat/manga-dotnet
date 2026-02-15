using System.Security.Claims;
using Manga.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Manga.Api.Middleware;

/// <summary>
/// Checks Redis blacklist for revoked JWT access tokens on each authenticated request.
/// </summary>
public class JwtBlacklistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService blacklist)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var jti = context.User.FindFirstValue("jti");
            if (jti is not null && await blacklist.IsBlacklistedAsync(jti))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = 401,
                    Title = "Unauthorized",
                    Detail = "Token has been revoked.",
                });
                return;
            }
        }

        await next(context);
    }
}
