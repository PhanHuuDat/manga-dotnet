using Manga.Application.Common.Models;

namespace Manga.Api.Extensions;

/// <summary>
/// Extension methods to convert Result objects to RFC 9457 ProblemDetails HTTP responses.
/// </summary>
public static class ResultExtensions
{
    private static string TitleForStatus(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        404 => "Not Found",
        422 => "Unprocessable Entity",
        _ => "Error"
    };

    public static IResult ToProblem(this Result result, int statusCode = 400)
    {
        return Results.Problem(
            detail: string.Join("; ", result.Errors),
            statusCode: statusCode,
            title: TitleForStatus(statusCode),
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = result.Errors
            });
    }

    public static IResult ToProblem<T>(this Result<T> result, int statusCode = 400)
    {
        return Results.Problem(
            detail: string.Join("; ", result.Errors),
            statusCode: statusCode,
            title: TitleForStatus(statusCode),
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = result.Errors
            });
    }
}
