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
    IImageProcessingService imageProcessing,
    IImageScrambleService imageScramble)
    : IRequestHandler<UploadAttachmentCommand, Result<AttachmentDto>>
{
    public async Task<Result<AttachmentDto>> Handle(
        UploadAttachmentCommand request, CancellationToken ct)
    {
        var subfolder = GetSubfolder(request.Type);

        // Process image (resize, convert to WebP, generate thumbnail)
        using var processed = await imageProcessing.ProcessAsync(
            request.FileStream, request.Type, ct);

        // Scramble chapter pages for anti-leak protection
        int? scrambleSeed = null;
        int? scrambleGridSize = null;
        Stream streamToStore = processed.ProcessedStream;
        ScrambleResult? scrambleResult = null;

        if (request.Type == AttachmentType.ChapterPage)
        {
            scrambleResult = await imageScramble.ScrambleAsync(
                processed.ProcessedStream, ct);
            streamToStore = scrambleResult.ScrambledStream;
            scrambleSeed = scrambleResult.Seed;
            scrambleGridSize = scrambleResult.GridSize;
        }

        try
        {
            // Store processed (and possibly scrambled) image
            var mainResult = await storage.StoreAsync(
                streamToStore,
                Path.ChangeExtension(request.FileName, ".webp"),
                processed.ProcessedContentType,
                subfolder,
                ct);

            // Store thumbnail if generated (thumbnail is NOT scrambled)
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
                ScrambleSeed = scrambleSeed,
                ScrambleGridSize = scrambleGridSize,
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync(ct);

            return Result<AttachmentDto>.Success(
                new AttachmentDto(attachment.Id, attachment.Url, attachment.ThumbnailUrl,
                    attachment.ContentType, attachment.FileSize));
        }
        finally
        {
            scrambleResult?.Dispose();
        }
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
