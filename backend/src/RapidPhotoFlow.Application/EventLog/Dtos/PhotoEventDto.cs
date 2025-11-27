namespace RapidPhotoFlow.Application.EventLog.Dtos;

/// <summary>
/// DTO for photo event log entries.
/// </summary>
public sealed record PhotoEventDto(
    Guid Id,
    Guid PhotoId,
    string EventType,
    string Message,
    DateTimeOffset CreatedAt,
    string? MetadataJson);

