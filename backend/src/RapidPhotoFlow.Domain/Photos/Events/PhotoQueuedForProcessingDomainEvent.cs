using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos.Events;

/// <summary>
/// Raised when a photo is queued for processing.
/// </summary>
public sealed record PhotoQueuedForProcessingDomainEvent(PhotoId PhotoId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

