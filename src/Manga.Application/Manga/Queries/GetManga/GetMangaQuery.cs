using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;

namespace Manga.Application.Manga.Queries.GetManga;

/// <summary>
/// Retrieves full manga detail by ID. Public endpoint (no auth required).
/// </summary>
public record GetMangaQuery(Guid Id) : IRequest<Result<MangaDetailDto>>;
