using Manga.Application.Manga.Commands.CreateManga;
using Manga.Application.Manga.Commands.DeleteManga;
using Manga.Application.Manga.Commands.UpdateManga;
using Manga.Application.Manga.Queries.GetManga;
using Manga.Application.Manga.Queries.GetMangaChapters;
using Manga.Application.Manga.Queries.GetTrendingManga;
using Manga.Application.Manga.Queries.ListManga;
using Manga.Application.Manga.Queries.SearchManga;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Api.Endpoints;

public static class MangaEndpoints
{
    public static void MapMangaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/manga").WithTags("Manga");

        group.MapPost("", CreateMangaAsync).RequireAuthorization();
        group.MapGet("", ListMangaAsync);
        group.MapGet("search", SearchMangaAsync);
        group.MapGet("trending", GetTrendingMangaAsync);
        group.MapGet("{id:guid}", GetMangaAsync);
        group.MapPut("{id:guid}", UpdateMangaAsync).RequireAuthorization();
        group.MapDelete("{id:guid}", DeleteMangaAsync).RequireAuthorization();
        group.MapGet("{id:guid}/chapters", GetMangaChaptersAsync);
    }

    private static async Task<IResult> CreateMangaAsync(
        CreateMangaCommand command, ISender sender)
    {
        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Created($"/api/manga/{result.Value}", result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> ListMangaAsync(
        int? page, int? pageSize, Guid? genreId, SeriesStatus? status,
        MangaSortBy? sortBy, string? search, ISender sender)
    {
        var query = new ListMangaQuery(
            page ?? 1,
            pageSize ?? 20,
            genreId,
            status,
            sortBy ?? MangaSortBy.Latest,
            search);

        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetMangaAsync(Guid id, ISender sender)
    {
        var result = await sender.Send(new GetMangaQuery(id));
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.NotFound(new { errors = result.Errors });
    }

    private static async Task<IResult> UpdateMangaAsync(
        Guid id, UpdateMangaCommand command, ISender sender)
    {
        var cmd = command with { Id = id };
        var result = await sender.Send(cmd);
        return result.Succeeded
            ? Results.NoContent()
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> DeleteMangaAsync(Guid id, ISender sender)
    {
        var result = await sender.Send(new DeleteMangaCommand(id));
        return result.Succeeded
            ? Results.NoContent()
            : Results.NotFound(new { errors = result.Errors });
    }

    private static async Task<IResult> GetMangaChaptersAsync(
        Guid id, int? page, int? pageSize, ISender sender)
    {
        var query = new GetMangaChaptersQuery(id, page ?? 1, pageSize ?? 50);
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.NotFound(new { errors = result.Errors });
    }

    private static async Task<IResult> SearchMangaAsync(
        string? q, int? page, int? pageSize, ISender sender)
    {
        var query = new SearchMangaQuery(q ?? "", page ?? 1, pageSize ?? 20);
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetTrendingMangaAsync(
        int? days, int? page, int? pageSize, ISender sender)
    {
        var query = new GetTrendingMangaQuery(days ?? 7, page ?? 1, pageSize ?? 20);
        var result = await sender.Send(query);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}
