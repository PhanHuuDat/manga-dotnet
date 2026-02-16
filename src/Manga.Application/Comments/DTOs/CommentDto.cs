using Manga.Domain.Enums;

namespace Manga.Application.Comments.DTOs;

/// <summary>
/// Comment DTO with threaded replies and author info.
/// </summary>
public record CommentDto(
    Guid Id,
    Guid UserId,
    string Username,
    string? AvatarUrl,
    string Content,
    int Likes,
    int Dislikes,
    Guid? MangaSeriesId,
    Guid? ChapterId,
    int? PageNumber,
    Guid? ParentId,
    int ReplyCount,
    List<CommentDto> Replies,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt);

/// <summary>
/// Response after toggling a reaction on a comment.
/// </summary>
public record ToggleReactionResponse(ReactionType? CurrentReaction, int Likes, int Dislikes);
