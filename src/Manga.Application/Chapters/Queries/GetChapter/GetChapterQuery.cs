using Manga.Application.Chapters.DTOs;
using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Chapters.Queries.GetChapter;

/// <summary>
/// Retrieves full chapter detail with pages. Public endpoint.
/// </summary>
public record GetChapterQuery(Guid Id) : IRequest<Result<ChapterDetailDto>>;
