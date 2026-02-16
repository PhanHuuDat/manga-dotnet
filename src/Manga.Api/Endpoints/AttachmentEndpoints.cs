using Manga.Application.Attachments.Commands.UploadAttachment;
using Manga.Application.Attachments.Queries.GetAttachmentFile;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static void MapAttachmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/attachments").WithTags("Attachments");

        group.MapPost("upload", UploadAsync)
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data");

        group.MapGet("{id:guid}/file", GetFileAsync);
    }

    private static async Task<IResult> UploadAsync(
        IFormFile file, AttachmentType type, ISender sender)
    {
        using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            stream, file.FileName, file.ContentType, file.Length, type);

        var result = await sender.Send(command);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { errors = result.Errors });
    }

    private static async Task<IResult> GetFileAsync(
        Guid id, HttpContext httpContext, ISender sender)
    {
        var result = await sender.Send(new GetAttachmentFileQuery(id));
        if (!result.Succeeded)
            return Results.NotFound(new { errors = result.Errors });

        httpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

        var file = result.Value!;
        return Results.File(
            file.FileStream,
            contentType: file.ContentType,
            enableRangeProcessing: true);
    }
}
