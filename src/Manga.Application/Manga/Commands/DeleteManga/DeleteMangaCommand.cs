using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Manga.Commands.DeleteManga;

/// <summary>
/// Soft-deletes a manga series. Requires MangaDelete permission (Moderator/Admin only).
/// </summary>
[RequirePermission(nameof(Permission.MangaDelete))]
public record DeleteMangaCommand(Guid Id) : IRequest<Result>;
