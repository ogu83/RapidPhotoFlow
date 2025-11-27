using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for Photo aggregate.
/// </summary>
public interface IPhotoRepository
{
    Task<Photo?> GetByIdAsync(PhotoId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Photo>> GetAllAsync(PhotoStatus? status = null, CancellationToken cancellationToken = default);
    Task AddAsync(Photo photo, CancellationToken cancellationToken = default);
    Task UpdateAsync(Photo photo, CancellationToken cancellationToken = default);
    Task DeleteAsync(Photo photo, CancellationToken cancellationToken = default);
    Task<Photo?> GetNextQueuedAsync(CancellationToken cancellationToken = default);
}

