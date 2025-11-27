namespace RapidPhotoFlow.Domain.Common.Abstractions;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

