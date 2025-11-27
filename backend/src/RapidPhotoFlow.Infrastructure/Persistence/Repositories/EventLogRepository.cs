using Microsoft.EntityFrameworkCore;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Domain.EventLog;

namespace RapidPhotoFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for EventLogEntry.
/// </summary>
public class EventLogRepository : IEventLogRepository
{
    private readonly ApplicationDbContext _context;

    public EventLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventLogEntry>> GetByPhotoIdAsync(Guid photoId, CancellationToken cancellationToken = default)
    {
        return await _context.EventLogEntries
            .Where(e => e.PhotoId == photoId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.EventLogEntries.AddAsync(entry, cancellationToken);
    }

    public async Task DeleteByPhotoIdAsync(Guid photoId, CancellationToken cancellationToken = default)
    {
        var entries = await _context.EventLogEntries
            .Where(e => e.PhotoId == photoId)
            .ToListAsync(cancellationToken);

        _context.EventLogEntries.RemoveRange(entries);
    }
}

