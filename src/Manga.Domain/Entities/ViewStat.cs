using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Daily aggregated view statistics per target entity.
/// Uses time-bucketed aggregation: 1 row per target per day instead of 1 row per view event.
/// Does not inherit AuditableEntity — stats data doesn't need soft-delete or audit fields.
/// </summary>
public class ViewStat : BaseEntity
{
    /// <summary>Type of the tracked entity (Series or Chapter).</summary>
    public ViewTargetType TargetType { get; set; }

    /// <summary>ID of the tracked manga series or chapter.</summary>
    public Guid TargetId { get; set; }

    /// <summary>The date this bucket aggregates views for.</summary>
    public DateOnly ViewDate { get; set; }

    /// <summary>Total view count for this target on this date.</summary>
    public long ViewCount { get; set; }

    /// <summary>Approximate unique viewer count (populated via Redis HyperLogLog).</summary>
    public long UniqueViewCount { get; set; }
}
