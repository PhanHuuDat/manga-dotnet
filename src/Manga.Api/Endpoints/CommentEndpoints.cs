using Manga.Application.Comments.Commands.CreateComment;
using Manga.Application.Comments.Commands.DeleteComment;
using Manga.Application.Comments.Commands.ToggleReaction;
using Manga.Application.Comments.Commands.UpdateComment;
using Manga.Application.Comments.Queries.ListComments;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Api.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/comments").WithTags("Comments");

        group.MapPost("", CreateAsync).RequireAuthorization();
        group.MapGet("", ListAsync);
        group.MapPut("{id:guid}", UpdateAsync).RequireAuthorization();
        group.MapDelete("{id:guid}", DeleteAsync).RequireAuthorization();
        group.MapPost("{id:guid}/reactions", ToggleReactionAsync).RequireAuthorization();
    }

    private static async Task<IResult> CreateAsync(
        CreateCommentCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/comments/{result.Value}", new { id = result.Value })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListAsync(
        Guid? mangaSeriesId, Guid? chapterId, int? pageNumber,
        int page, int pageSize, ISender sender)
    {
        var result = await sender.Send(new ListCommentsQuery(
            mangaSeriesId, chapterId, pageNumber,
            page > 0 ? page : 1,
            pageSize is > 0 and <= 50 ? pageSize : 20));
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id, UpdateCommentRequest body, ISender sender)
    {
        var result = await sender.Send(new UpdateCommentCommand(id, body.Content));
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> DeleteAsync(Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteCommentCommand(id));
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ToggleReactionAsync(
        Guid id, ToggleReactionRequest body, ISender sender)
    {
        var result = await sender.Send(new ToggleReactionCommand(id, body.ReactionType));
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}

internal record UpdateCommentRequest(string Content);
internal record ToggleReactionRequest(ReactionType ReactionType);
