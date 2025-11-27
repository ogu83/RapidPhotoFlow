namespace RapidPhotoFlow.Domain.EventLog;

/// <summary>
/// Represents an event log entry for tracking photo workflow events.
/// </summary>
public sealed class EventLogEntry
{
    private EventLogEntry() { } // For EF Core

    public Guid Id { get; private set; }
    public Guid PhotoId { get; private set; }
    public string EventType { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// Creates a new event log entry.
    /// </summary>
    public static EventLogEntry Create(
        Guid photoId,
        string eventType,
        string message,
        DateTimeOffset createdAt,
        string? metadataJson = null)
    {
        return new EventLogEntry
        {
            Id = Guid.NewGuid(),
            PhotoId = photoId,
            EventType = eventType,
            Message = message,
            CreatedAt = createdAt,
            MetadataJson = metadataJson
        };
    }
}

