using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos.Events;

/// <summary>
/// Raised when processing starts for a photo.
/// </summary>
public sealed record PhotoProcessingStartedDomainEvent(PhotoId PhotoId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

