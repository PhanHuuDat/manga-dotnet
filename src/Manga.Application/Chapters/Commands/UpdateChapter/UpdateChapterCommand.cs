using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Chapters.Commands.UpdateChapter;

/// <summary>
/// Updates a chapter's metadata or replaces its pages. Requires ChapterUpdate permission.
/// </summary>
[RequirePermission(nameof(Permission.ChapterUpdate))]
public record UpdateChapterCommand(
    Guid Id,
    string? Title,
    DateTimeOffset? PublishedAt,
    List<Guid>? PageImageIds) : IRequest<Result>;
