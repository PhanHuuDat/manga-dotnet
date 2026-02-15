namespace Manga.Application.Common.Models;

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public record PagedResponse<T>(
    List<T> Data,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNext);
