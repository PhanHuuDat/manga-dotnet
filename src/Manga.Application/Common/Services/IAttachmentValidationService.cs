using Manga.Application.Common.Models;

namespace Manga.Application.Common.Services;

public interface IAttachmentValidationService
{
    /// <summary>Validates that the attachment exists. Returns failure Result if not found.</summary>
    Task<Result?> ValidateExistsAsync(Guid? attachmentId, string fieldName, CancellationToken ct);
}
