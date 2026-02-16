using Manga.Application.ReadingHistories.Commands.ClearReadingHistory;
using Manga.Application.ReadingHistories.Commands.UpsertReadingHistory;
using Manga.Application.ReadingHistories.Queries.GetResumePoint;
using Manga.Application.ReadingHistories.Queries.ListReadingHistory;
using MediatR;

namespace Manga.Api.Endpoints;

public static class ReadingHistoryEndpoints
{
    public static void MapReadingHistoryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/reading-history")
            .WithTags("ReadingHistory")
            .RequireAuthorization();

        group.MapPost("", UpsertAsync);
        group.MapGet("", ListAsync);
        group.MapGet("{mangaId:guid}", GetResumePointAsync);
        group.MapDelete("{mangaId:guid}", ClearAsync);
    }

    private static async Task<IResult> UpsertAsync(
        UpsertReadingHistoryCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(new { id = result.Value })
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListAsync(
        int page, int pageSize, ISender sender)
    {
        var result = await sender.Send(new ListReadingHistoryQuery(
            page > 0 ? page : 1,
            pageSize is > 0 and <= 50 ? pageSize : 20));
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetResumePointAsync(
        Guid mangaId, ISender sender)
    {
        var result = await sender.Send(new GetResumePointQuery(mangaId));
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ClearAsync(
        Guid mangaId, ISender sender)
    {
        var result = await sender.Send(new ClearReadingHistoryCommand(mangaId));
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }
}
