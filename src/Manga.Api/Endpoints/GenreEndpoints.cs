using Manga.Application.Genres.Queries.ListGenres;
using MediatR;

namespace Manga.Api.Endpoints;

public static class GenreEndpoints
{
    public static void MapGenreEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/genres").WithTags("Genres");

        group.MapGet("", ListGenresAsync);
    }

    private static async Task<IResult> ListGenresAsync(ISender sender)
    {
        var result = await sender.Send(new ListGenresQuery());
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }
}
