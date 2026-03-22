using Manga.Api.Extensions;
using Manga.Application.Chapters.Commands.CreateChapter;
using Manga.Application.Chapters.Commands.DeleteChapter;
using Manga.Application.Chapters.Commands.UpdateChapter;
using Manga.Application.Chapters.Queries.GetChapter;
using MediatR;

namespace Manga.Api.Endpoints;

public static class ChapterEndpoints
{
    public static void MapChapterEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/chapters").WithTags("Chapters");

        group.MapPost("", CreateChapterAsync).RequireAuthorization();
        group.MapGet("{id:guid}", GetChapterAsync);
        group.MapPut("{id:guid}", UpdateChapterAsync).RequireAuthorization();
        group.MapDelete("{id:guid}", DeleteChapterAsync).RequireAuthorization();
    }

    private static async Task<IResult> CreateChapterAsync(
        CreateChapterCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/chapters/{result.Value}", result.Value)
            : result.ToProblem();
    }

    private static async Task<IResult> GetChapterAsync(Guid id, ISender sender)
    {
        var result = await sender.Send(new GetChapterQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value)
            : result.ToProblem(404);
    }

    private static async Task<IResult> UpdateChapterAsync(
        Guid id, UpdateChapterCommand command, ISender sender)
    {
        var cmd = command with { Id = id };
        var result = await sender.Send(cmd);
        return result.Succeeded
            ? Results.NoContent()
            : result.ToProblem();
    }

    private static async Task<IResult> DeleteChapterAsync(Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteChapterCommand(id));
        return result.Succeeded
            ? Results.NoContent()
            : result.ToProblem(404);
    }
}
