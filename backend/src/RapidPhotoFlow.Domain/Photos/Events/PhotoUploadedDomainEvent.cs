using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos.Events;

/// <summary>
/// Raised when a photo is uploaded.
/// </summary>
public sealed record PhotoUploadedDomainEvent(PhotoId PhotoId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

