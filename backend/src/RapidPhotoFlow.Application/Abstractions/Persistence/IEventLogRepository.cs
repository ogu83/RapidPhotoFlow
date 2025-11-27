using RapidPhotoFlow.Domain.EventLog;

namespace RapidPhotoFlow.Application.Abstractions.Persistence;

/// <summary>
/// Repository interface for EventLogEntry.
/// </summary>
public interface IEventLogRepository
{
    Task<IReadOnlyList<EventLogEntry>> GetByPhotoIdAsync(Guid photoId, CancellationToken cancellationToken = default);
    Task AddAsync(EventLogEntry entry, CancellationToken cancellationToken = default);
}

