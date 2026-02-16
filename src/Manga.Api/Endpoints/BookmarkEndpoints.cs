using Manga.Application.Bookmarks.Commands.DeleteBookmark;
using Manga.Application.Bookmarks.Commands.ToggleBookmark;
using Manga.Application.Bookmarks.Queries.CheckBookmark;
using Manga.Application.Bookmarks.Queries.ListBookmarks;
using MediatR;

namespace Manga.Api.Endpoints;

public static class BookmarkEndpoints
{
    public static void MapBookmarkEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookmarks")
            .WithTags("Bookmarks")
            .RequireAuthorization();

        group.MapPost("", ToggleAsync);
        group.MapGet("", ListAsync);
        group.MapGet("check/{mangaId:guid}", CheckAsync);
        group.MapDelete("{id:guid}", DeleteAsync);
    }

    private static async Task<IResult> ToggleAsync(
        ToggleBookmarkCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListAsync(
        int page, int pageSize, ISender sender)
    {
        var result = await sender.Send(new ListBookmarksQuery(
            page > 0 ? page : 1,
            pageSize is > 0 and <= 50 ? pageSize : 20));
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CheckAsync(
        Guid mangaId, ISender sender)
    {
        var result = await sender.Send(new CheckBookmarkQuery(mangaId));
        return Results.Ok(new { isBookmarked = result.Value });
    }

    private static async Task<IResult> DeleteAsync(
        Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteBookmarkCommand(id));
        return result.Succeeded
            ? Results.NoContent()
            : Results.NotFound(new { errors = result.Errors });
    }
}
