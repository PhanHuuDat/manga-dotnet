namespace Manga.Domain.Common;

/// <summary>
/// Auditable entity with created/modified timestamps and soft delete.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    protected void UpdateLastModified()
    {
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}
