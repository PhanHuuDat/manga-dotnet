namespace Manga.Application.Attachments.DTOs;

/// <summary>
/// DTO returned after uploading an attachment.
/// </summary>
public record AttachmentDto(Guid Id, string Url, string? ThumbnailUrl, string ContentType, long FileSize);
