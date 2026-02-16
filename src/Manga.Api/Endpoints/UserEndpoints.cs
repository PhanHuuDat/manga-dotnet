using Manga.Application.Users.Commands.UpdateProfile;
using Manga.Application.Users.Commands.UploadAvatar;
using Manga.Application.Users.Queries.GetUserStats;
using MediatR;

namespace Manga.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");

        group.MapPut("profile", UpdateProfileAsync)
            .RequireAuthorization();

        group.MapPost("avatar", UploadAvatarAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data");

        group.MapGet("stats", GetStatsAsync)
            .RequireAuthorization();
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { message = "Profile updated successfully." })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> UploadAvatarAsync(
        IFormFile file, ISender sender)
    {
        using var stream = file.OpenReadStream();
        var command = new UploadAvatarCommand(
            stream, file.FileName, file.ContentType, file.Length);

        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { avatarUrl = result.Value })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetStatsAsync(ISender sender)
    {
        var result = await sender.Send(new GetUserStatsQuery());
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}
