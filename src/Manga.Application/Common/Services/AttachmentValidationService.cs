using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Common.Services;

public class AttachmentValidationService(IAppDbContext db) : IAttachmentValidationService
{
    public async Task<Result?> ValidateExistsAsync(
        Guid? attachmentId, string fieldName, CancellationToken ct)
    {
        if (!attachmentId.HasValue)
            return null;

        var exists = await db.Attachments.AnyAsync(a => a.Id == attachmentId.Value, ct);
        return exists ? null : Result.Failure($"{fieldName} attachment not found.");
    }
}
