namespace Manga.Application.Admin.Queries.ListAdminComments;

/// <summary>
/// Comment summary for admin moderation view, including soft-deleted entries.
/// </summary>
public record AdminCommentDto(
    Guid Id,
    string Content,
    string Username,
    Guid UserId,
    string? MangaTitle,
    DateTimeOffset CreatedDate,
    bool IsDeleted);
