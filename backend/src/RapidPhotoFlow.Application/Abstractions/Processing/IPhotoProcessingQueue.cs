using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Abstractions.Processing;

/// <summary>
/// Interface for photo processing queue.
/// </summary>
public interface IPhotoProcessingQueue
{
    Task EnqueueAsync(PhotoId photoId, CancellationToken cancellationToken = default);
    Task<PhotoId?> DequeueAsync(CancellationToken cancellationToken = default);
}

