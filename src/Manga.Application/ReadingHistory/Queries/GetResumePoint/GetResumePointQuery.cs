using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Application.ReadingHistories.DTOs;
using MediatR;

namespace Manga.Application.ReadingHistories.Queries.GetResumePoint;

/// <summary>
/// Get the resume point (chapter + page) for a specific manga.
/// </summary>
[Authorize]
public record GetResumePointQuery(Guid MangaSeriesId) : IRequest<Result<ResumePointDto?>>;
