using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Attachments.Queries.GetAttachmentFile;

public class GetAttachmentFileQueryHandler(
    IAppDbContext db,
    IFileStorageService storage)
    : IRequestHandler<GetAttachmentFileQuery, Result<AttachmentFileResult>>
{
    public async Task<Result<AttachmentFileResult>> Handle(
        GetAttachmentFileQuery request, CancellationToken ct)
    {
        var attachment = await db.Attachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

        if (attachment is null)
            return Result<AttachmentFileResult>.Failure("Attachment not found.");

        var stream = await storage.GetAsync(attachment.StoragePath, ct);
        if (stream is null)
            return Result<AttachmentFileResult>.Failure("File not found on storage.");

        return Result<AttachmentFileResult>.Success(
            new AttachmentFileResult(stream, attachment.ContentType, attachment.FileName));
    }
}
