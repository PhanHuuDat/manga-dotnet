using Manga.Application.Admin.Commands.UpdateUserRole;
using Manga.Application.Admin.Commands.UpdateUserStatus;
using Manga.Application.Admin.Queries.GetAdminStats;
using Manga.Application.Admin.Queries.ListAdminComments;
using Manga.Application.Admin.Queries.ListUsers;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization();

        group.MapGet("stats", GetStatsAsync);
        group.MapGet("users", ListUsersAsync);
        group.MapPut("users/{id:guid}/role", UpdateUserRoleAsync);
        group.MapPut("users/{id:guid}/status", UpdateUserStatusAsync);
        group.MapGet("comments", ListCommentsAsync);
    }

    private static async Task<IResult> GetStatsAsync(ISender sender)
    {
        var result = await sender.Send(new GetAdminStatsQuery());
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListUsersAsync(
        int? page, int? pageSize, string? search, UserRole? role, ISender sender)
    {
        var query = new ListUsersQuery(page ?? 1, pageSize ?? 20, search, role);
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> UpdateUserRoleAsync(
        Guid id, UpdateUserRoleCommand body, ISender sender)
    {
        var cmd = body with { UserId = id };
        var result = await sender.Send(cmd);
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> UpdateUserStatusAsync(
        Guid id, UpdateUserStatusCommand body, ISender sender)
    {
        var cmd = body with { UserId = id };
        var result = await sender.Send(cmd);
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListCommentsAsync(
        int? page, int? pageSize, string? search, Guid? userId, ISender sender)
    {
        var query = new ListAdminCommentsQuery(page ?? 1, pageSize ?? 20, search, userId);
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}
