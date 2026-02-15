namespace Manga.Domain.Exceptions;

/// <summary>
/// Thrown when an authenticated user lacks required permissions.
/// </summary>
public class ForbiddenAccessException()
    : Exception("Access denied. Insufficient permissions.");
