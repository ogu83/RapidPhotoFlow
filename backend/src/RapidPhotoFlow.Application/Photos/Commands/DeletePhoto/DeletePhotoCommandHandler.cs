using MediatR;
using Microsoft.Extensions.Logging;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Abstractions.Storage;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Photos.Commands.DeletePhoto;

/// <summary>
/// Handler for DeletePhotoCommand.
/// Handles deletion differently based on photo status:
/// - Queued/Processing: Marks as cancelled (removed from processing)
/// - Processed/Failed: Deletes from database and storage
/// </summary>
public sealed class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand, DeletePhotoResult>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IEventLogRepository _eventLogRepository;
    private readonly IPhotoStorage _photoStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePhotoCommandHandler> _logger;

    public DeletePhotoCommandHandler(
        IPhotoRepository photoRepository,
        IEventLogRepository eventLogRepository,
        IPhotoStorage photoStorage,
        IUnitOfWork unitOfWork,
        ILogger<DeletePhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _eventLogRepository = eventLogRepository;
        _photoStorage = photoStorage;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DeletePhotoResult> Handle(DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var photoId = new PhotoId(request.PhotoId);
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);

        if (photo is null)
        {
            return new DeletePhotoResult(false, "Photo not found");
        }

        var fileName = photo.FileName;
        var status = photo.Status;

        _logger.LogInformation("Deleting photo - PhotoId: {PhotoId}, FileName: {FileName}, Status: {Status}",
            request.PhotoId, fileName, status);

        // Delete event log entries for this photo
        await _eventLogRepository.DeleteByPhotoIdAsync(request.PhotoId, cancellationToken);

        // Delete the photo record from database
        await _photoRepository.DeleteAsync(photo, cancellationToken);

        // For processed photos, also delete the file from storage
        if (status == PhotoStatus.Processed || status == PhotoStatus.Failed)
        {
            try
            {
                await _photoStorage.DeleteAsync(photo.StoragePath, cancellationToken);
                _logger.LogInformation("Deleted photo file from storage - PhotoId: {PhotoId}, StoragePath: {StoragePath}",
                    request.PhotoId, photo.StoragePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete photo file from storage - PhotoId: {PhotoId}, StoragePath: {StoragePath}",
                    request.PhotoId, photo.StoragePath);
                // Continue anyway - the DB record will be deleted
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Photo deleted successfully - PhotoId: {PhotoId}, FileName: {FileName}",
            request.PhotoId, fileName);

        return new DeletePhotoResult(true, $"Photo '{fileName}' deleted successfully");
    }
}

