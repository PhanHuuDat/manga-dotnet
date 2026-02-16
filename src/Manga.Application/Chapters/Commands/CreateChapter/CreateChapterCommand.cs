using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Chapters.Commands.CreateChapter;

/// <summary>
/// Creates a new chapter for a manga series. Requires ChapterCreate permission.
/// </summary>
[RequirePermission(nameof(Permission.ChapterCreate))]
public record CreateChapterCommand(
    Guid MangaSeriesId,
    decimal ChapterNumber,
    string? Title,
    DateTimeOffset PublishedAt,
    List<Guid> PageImageIds) : IRequest<Result<Guid>>;
