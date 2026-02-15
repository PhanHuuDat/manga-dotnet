using System.Security.Claims;
using Manga.Application.Auth.Commands.ForgotPassword;
using Manga.Application.Auth.Commands.Login;
using Manga.Application.Auth.Commands.Logout;
using Manga.Application.Auth.Commands.Register;
using Manga.Application.Auth.Commands.ResetPassword;
using Manga.Application.Auth.Commands.VerifyEmail;
using Manga.Application.Auth.Queries.GetCurrentUser;
using MediatR;
using RefreshTokenCommand = Manga.Application.Auth.Commands.RefreshToken.RefreshTokenCommand;

namespace Manga.Api.Endpoints;

public static class AuthEndpoints
{
    private const string RefreshCookieName = "refreshToken";

    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();
        group.MapPost("/verify-email", VerifyEmailAsync);
        group.MapPost("/forgot-password", ForgotPasswordAsync);
        group.MapPost("/reset-password", ResetPasswordAsync);
        group.MapGet("/me", GetCurrentUserAsync).RequireAuthorization();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created()
            : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> LoginAsync(
        LoginCommand command, ISender sender, HttpContext http, IConfiguration config)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return Results.UnprocessableEntity(new { errors = result.Errors });

        var data = result.Value!;
        SetRefreshCookie(http, data.RawRefreshToken, data.RefreshTokenExpiresAt, config);
        return Results.Ok(data.Auth);
    }

    private static async Task<IResult> RefreshAsync(
        ISender sender, HttpContext http, IConfiguration config)
    {
        var rawToken = http.Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(rawToken))
            return Results.Unauthorized();

        var result = await sender.Send(new RefreshTokenCommand(rawToken));
        if (!result.Succeeded)
        {
            ClearRefreshCookie(http);
            return Results.Unauthorized();
        }

        var data = result.Value!;
        SetRefreshCookie(http, data.RawRefreshToken, data.RefreshTokenExpiresAt, config);
        return Results.Ok(data.Auth);
    }

    private static async Task<IResult> LogoutAsync(
        ISender sender, HttpContext http)
    {
        var jti = http.User.FindFirstValue("jti") ?? string.Empty;
        var expClaim = http.User.FindFirstValue("exp");
        var expiry = expClaim is not null
            ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim))
            : DateTimeOffset.UtcNow;

        await sender.Send(new LogoutCommand(jti, expiry));
        ClearRefreshCookie(http);
        return Results.NoContent();
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.Ok(); // Always 200 to prevent enumeration
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
    }

    private static async Task<IResult> GetCurrentUserAsync(ISender sender)
    {
        var result = await sender.Send(new GetCurrentUserQuery());
        return result.Succeeded ? Results.Ok(result.Value) : Results.NotFound(result.Errors);
    }

    private static void SetRefreshCookie(
        HttpContext http, string token, DateTimeOffset expires, IConfiguration config)
    {
        var isDev = config.GetValue<bool>("Email:UseDev"); // proxy for dev env
        http.Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDev ? SameSiteMode.Lax : SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = expires,
        });
    }

    private static void ClearRefreshCookie(HttpContext http)
    {
        http.Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Path = "/api/auth",
        });
    }
}
