using Manga.Domain.Enums;

namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Abstraction for Redis HyperLogLog-based unique view tracking.
/// </summary>
public interface IViewTrackingService
{
    /// <summary>
    /// Adds a viewer identifier to the HyperLogLog for the given target/date
    /// and returns the approximate unique viewer count.
    /// </summary>
    Task<long> TrackAndGetUniqueCountAsync(
        ViewTargetType targetType, Guid targetId, DateOnly viewDate,
        string viewerIdentifier, CancellationToken ct = default);
}
