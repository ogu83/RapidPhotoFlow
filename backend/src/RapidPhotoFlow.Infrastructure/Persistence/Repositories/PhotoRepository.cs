using Microsoft.EntityFrameworkCore;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Photo aggregate.
/// </summary>
public class PhotoRepository : IPhotoRepository
{
    private readonly ApplicationDbContext _context;

    public PhotoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Photo?> GetByIdAsync(PhotoId id, CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Photo>> GetAllAsync(PhotoStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Photos.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Photo photo, CancellationToken cancellationToken = default)
    {
        await _context.Photos.AddAsync(photo, cancellationToken);
    }

    public Task UpdateAsync(Photo photo, CancellationToken cancellationToken = default)
    {
        _context.Photos.Update(photo);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Photo photo, CancellationToken cancellationToken = default)
    {
        _context.Photos.Remove(photo);
        return Task.CompletedTask;
    }

    public async Task<Photo?> GetNextQueuedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .Where(p => p.Status == PhotoStatus.Queued)
            .OrderBy(p => p.UploadedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

