using Manga.Application.Attachments.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Attachments.Commands.UploadAttachment;

public class UploadAttachmentCommandHandler(
    IAppDbContext db,
    IFileStorageService storage,
    IImageProcessingService imageProcessing)
    : IRequestHandler<UploadAttachmentCommand, Result<AttachmentDto>>
{
    public async Task<Result<AttachmentDto>> Handle(
        UploadAttachmentCommand request, CancellationToken ct)
    {
        var subfolder = GetSubfolder(request.Type);

        // Process image (resize, convert to WebP, generate thumbnail)
        using var processed = await imageProcessing.ProcessAsync(
            request.FileStream, request.Type, ct);

        // Store processed image
        var mainResult = await storage.StoreAsync(
            processed.ProcessedStream,
            Path.ChangeExtension(request.FileName, ".webp"),
            processed.ProcessedContentType,
            subfolder,
            ct);

        // Store thumbnail if generated
        FileStorageResult? thumbResult = null;
        if (processed.ThumbnailStream is not null)
        {
            var thumbName = Path.GetFileNameWithoutExtension(request.FileName) + "_thumb.webp";
            thumbResult = await storage.StoreAsync(
                processed.ThumbnailStream,
                thumbName,
                "image/webp",
                subfolder,
                ct);
        }

        var attachment = new Attachment
        {
            FileName = request.FileName,
            StoragePath = mainResult.StoragePath,
            Url = mainResult.Url,
            ContentType = processed.ProcessedContentType,
            FileSize = mainResult.FileSize,
            Type = request.Type,
            ThumbnailUrl = thumbResult?.Url,
            ThumbnailStoragePath = thumbResult?.StoragePath,
        };

        db.Attachments.Add(attachment);
        await db.SaveChangesAsync(ct);

        return Result<AttachmentDto>.Success(
            new AttachmentDto(attachment.Id, attachment.Url, attachment.ThumbnailUrl,
                attachment.ContentType, attachment.FileSize));
    }

    private static string GetSubfolder(AttachmentType type) => type switch
    {
        AttachmentType.Avatar => "avatar",
        AttachmentType.MangaCover => "manga-cover",
        AttachmentType.MangaBanner => "manga-banner",
        AttachmentType.ChapterPage => "chapter-page",
        AttachmentType.PersonPhoto => "person-photo",
        _ => "other",
    };
}
