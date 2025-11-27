using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos.Events;

/// <summary>
/// Raised when processing fails for a photo.
/// </summary>
public sealed record PhotoProcessingFailedDomainEvent(PhotoId PhotoId, string ErrorMessage) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

