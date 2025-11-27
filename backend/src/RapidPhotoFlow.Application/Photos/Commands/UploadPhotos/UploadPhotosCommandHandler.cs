using MediatR;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Abstractions.Processing;
using RapidPhotoFlow.Application.Abstractions.Storage;
using RapidPhotoFlow.Application.Photos.Dtos;
using RapidPhotoFlow.Domain.EventLog;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

/// <summary>
/// Handler for UploadPhotosCommand.
/// </summary>
public sealed class UploadPhotosCommandHandler : IRequestHandler<UploadPhotosCommand, IReadOnlyCollection<PhotoDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IEventLogRepository _eventLogRepository;
    private readonly IPhotoStorage _photoStorage;
    private readonly IPhotoProcessingQueue _processingQueue;
    private readonly IUnitOfWork _unitOfWork;

    public UploadPhotosCommandHandler(
        IPhotoRepository photoRepository,
        IEventLogRepository eventLogRepository,
        IPhotoStorage photoStorage,
        IPhotoProcessingQueue processingQueue,
        IUnitOfWork unitOfWork)
    {
        _photoRepository = photoRepository;
        _eventLogRepository = eventLogRepository;
        _photoStorage = photoStorage;
        _processingQueue = processingQueue;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<PhotoDto>> Handle(
        UploadPhotosCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<PhotoDto>();
        var photoIdsToQueue = new List<PhotoId>();

        foreach (var item in request.Photos)
        {
            // Store the file
            var storagePath = await _photoStorage.SaveAsync(
                item.Content,
                item.FileName,
                cancellationToken);

            // Create the photo entity
            var photoId = PhotoId.New();
            var now = DateTimeOffset.UtcNow;

            var photo = Photo.CreateNew(
                photoId,
                item.FileName,
                item.ContentType,
                item.SizeBytes,
                storagePath,
                now);

            // Queue for processing (sets status)
            photo.QueueForProcessing();

            // Persist the photo
            await _photoRepository.AddAsync(photo, cancellationToken);

            // Log events
            await _eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "Uploaded", $"Photo '{item.FileName}' uploaded", now),
                cancellationToken);

            await _eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "Queued", $"Photo '{item.FileName}' queued for processing", now),
                cancellationToken);

            // Clear domain events after handling
            photo.ClearDomainEvents();

            results.Add(MapToDto(photo));
            photoIdsToQueue.Add(photoId);
        }

        // IMPORTANT: Save to database BEFORE enqueueing to avoid race condition
        // where worker tries to process photos that don't exist yet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Now that photos are persisted, add them to the processing queue
        foreach (var photoId in photoIdsToQueue)
        {
            await _processingQueue.EnqueueAsync(photoId, cancellationToken);
        }

        return results;
    }

    private static PhotoDto MapToDto(Photo photo) => new(
        photo.Id.Value,
        photo.FileName,
        photo.ContentType,
        photo.SizeBytes,
        photo.Status.ToString(),
        photo.StoragePath,
        photo.UploadedAt,
        photo.ProcessingStartedAt,
        photo.ProcessingCompletedAt,
        photo.ErrorMessage);
}
