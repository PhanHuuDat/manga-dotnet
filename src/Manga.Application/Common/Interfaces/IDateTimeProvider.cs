namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Abstraction for date/time to support testability.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
