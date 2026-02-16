using Manga.Application.Common.Models;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Views.Commands.TrackView;

/// <summary>
/// Tracks a view event for a manga series or chapter.
/// Public endpoint — no auth required.
/// ViewerIdentifier is set by the endpoint (userId or hashed IP).
/// </summary>
public record TrackViewCommand(
    ViewTargetType TargetType,
    Guid TargetId,
    string ViewerIdentifier) : IRequest<Result>;
