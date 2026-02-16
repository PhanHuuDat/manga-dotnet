using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser,
    IFileStorageService storage,
    IImageProcessingService imageProcessing)
    : IRequestHandler<UploadAvatarCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        UploadAvatarCommand request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return Result<string>.Failure("User not authenticated.");

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result<string>.Failure("User not found.");

        // Process image (resize, convert to WebP, generate thumbnail)
        using var processed = await imageProcessing.ProcessAsync(
            request.FileStream, AttachmentType.Avatar, ct);

        // Store processed image
        var mainResult = await storage.StoreAsync(
            processed.ProcessedStream,
            Path.ChangeExtension(request.FileName, ".webp"),
            processed.ProcessedContentType,
            "avatar",
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
                "avatar",
                ct);
        }

        // Create attachment entity
        var attachment = new Attachment
        {
            FileName = request.FileName,
            StoragePath = mainResult.StoragePath,
            Url = mainResult.Url,
            ContentType = processed.ProcessedContentType,
            FileSize = mainResult.FileSize,
            Type = AttachmentType.Avatar,
            ThumbnailUrl = thumbResult?.Url,
            ThumbnailStoragePath = thumbResult?.StoragePath,
        };

        db.Attachments.Add(attachment);

        // Update user's avatar
        user.AvatarId = attachment.Id;

        await db.SaveChangesAsync(ct);

        return Result<string>.Success(attachment.Url);
    }
}
