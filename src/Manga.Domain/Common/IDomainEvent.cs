using MediatR;

namespace Manga.Domain.Common;

/// <summary>
/// Marker interface for domain events dispatched via MediatR.
/// </summary>
public interface IDomainEvent : INotification;
