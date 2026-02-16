using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Chapters.Commands.DeleteChapter;

/// <summary>
/// Soft-deletes a chapter. Requires ChapterDelete permission (Moderator/Admin).
/// </summary>
[RequirePermission(nameof(Permission.ChapterDelete))]
public record DeleteChapterCommand(Guid Id) : IRequest<Result>;
