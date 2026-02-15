namespace Manga.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist.
/// </summary>
public class NotFoundException(string name, object key)
    : DomainException($"Entity \"{name}\" ({key}) was not found.");
