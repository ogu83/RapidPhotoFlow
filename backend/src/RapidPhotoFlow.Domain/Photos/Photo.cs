using RapidPhotoFlow.Domain.Common.Abstractions;
using RapidPhotoFlow.Domain.Photos.Events;

namespace RapidPhotoFlow.Domain.Photos;

/// <summary>
/// Photo aggregate root - encapsulates photo state and business rules.
/// </summary>
public sealed class Photo
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private Photo() { } // For EF Core

    public PhotoId Id { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string StoragePath { get; private set; } = null!;
    public PhotoStatus Status { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }
    public DateTimeOffset? ProcessingStartedAt { get; private set; }
    public DateTimeOffset? ProcessingCompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Creates a new Photo entity.
    /// </summary>
    public static Photo CreateNew(
        PhotoId id,
        string fileName,
        string contentType,
        long sizeBytes,
        string storagePath,
        DateTimeOffset uploadedAtUtc)
    {
        var photo = new Photo
        {
            Id = id,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            Status = PhotoStatus.Uploaded,
            UploadedAt = uploadedAtUtc
        };

        photo.AddDomainEvent(new PhotoUploadedDomainEvent(photo.Id));
        return photo;
    }

    /// <summary>
    /// Queues the photo for processing.
    /// </summary>
    public void QueueForProcessing()
    {
        if (Status != PhotoStatus.Uploaded)
        {
            throw new InvalidOperationException($"Cannot queue photo from status {Status}.");
        }

        Status = PhotoStatus.Queued;
        AddDomainEvent(new PhotoQueuedForProcessingDomainEvent(Id));
    }

    /// <summary>
    /// Starts processing the photo.
    /// </summary>
    public void StartProcessing(DateTimeOffset startedAtUtc)
    {
        if (Status is not PhotoStatus.Queued)
        {
            throw new InvalidOperationException($"Cannot start processing from status {Status}.");
        }

        Status = PhotoStatus.Processing;
        ProcessingStartedAt = startedAtUtc;
        AddDomainEvent(new PhotoProcessingStartedDomainEvent(Id));
    }

    /// <summary>
    /// Marks the photo as successfully processed.
    /// </summary>
    public void MarkProcessed(DateTimeOffset completedAtUtc)
    {
        if (Status is not PhotoStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot complete processing from status {Status}.");
        }

        Status = PhotoStatus.Processed;
        ProcessingCompletedAt = completedAtUtc;
        ErrorMessage = null;

        AddDomainEvent(new PhotoProcessingCompletedDomainEvent(Id));
    }

    /// <summary>
    /// Marks the photo processing as failed.
    /// </summary>
    public void MarkFailed(string errorMessage, DateTimeOffset failedAtUtc)
    {
        if (Status is not PhotoStatus.Queued and not PhotoStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot mark failed from status {Status}.");
        }

        Status = PhotoStatus.Failed;
        ProcessingCompletedAt = failedAtUtc;
        ErrorMessage = errorMessage;

        AddDomainEvent(new PhotoProcessingFailedDomainEvent(Id, errorMessage));
    }

    /// <summary>
    /// Clears all domain events (called after persistence).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
}

