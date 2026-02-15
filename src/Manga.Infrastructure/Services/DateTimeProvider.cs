using Manga.Application.Common.Interfaces;

namespace Manga.Infrastructure.Services;

/// <summary>
/// System clock implementation of IDateTimeProvider.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
