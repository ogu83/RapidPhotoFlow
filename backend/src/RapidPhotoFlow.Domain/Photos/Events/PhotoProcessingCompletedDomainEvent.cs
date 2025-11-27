using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos.Events;

/// <summary>
/// Raised when processing completes successfully for a photo.
/// </summary>
public sealed record PhotoProcessingCompletedDomainEvent(PhotoId PhotoId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

