using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Abstractions.Processing;
using RapidPhotoFlow.Domain.EventLog;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Infrastructure.Processing;

/// <summary>
/// Background worker that processes photos from the queue.
/// </summary>
public class PhotoProcessingWorker : BackgroundService
{
    private readonly IPhotoProcessingQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PhotoProcessingWorker> _logger;
    private readonly Random _random = new();

    public PhotoProcessingWorker(
        IPhotoProcessingQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<PhotoProcessingWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Photo Processing Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var photoId = await _queue.DequeueAsync(stoppingToken);

                if (photoId.HasValue)
                {
                    await ProcessPhotoAsync(photoId.Value, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing photo from queue");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Photo Processing Worker stopped");
    }

    private async Task ProcessPhotoAsync(PhotoId photoId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var photoRepository = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();
        var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var photo = await photoRepository.GetByIdAsync(photoId, cancellationToken);

        if (photo is null)
        {
            _logger.LogWarning("Photo {PhotoId} not found for processing", photoId);
            return;
        }

        if (photo.Status != PhotoStatus.Queued)
        {
            _logger.LogWarning("Photo {PhotoId} is not in Queued status, skipping", photoId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        try
        {
            // Start processing
            photo.StartProcessing(now);
            await photoRepository.UpdateAsync(photo, cancellationToken);
            await eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "ProcessingStarted", $"Processing started for '{photo.FileName}'", now),
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Started processing photo {PhotoId}: {FileName}", photoId, photo.FileName);

            // Simulate processing with random delay (2-5 seconds)
            var delay = _random.Next(2000, 5000);
            await Task.Delay(delay, cancellationToken);

            // Simulate occasional failures (10% chance)
            if (_random.Next(100) < 10)
            {
                throw new InvalidOperationException("Simulated processing failure");
            }

            // Mark as processed
            var completedAt = DateTimeOffset.UtcNow;
            photo.MarkProcessed(completedAt);
            await photoRepository.UpdateAsync(photo, cancellationToken);
            await eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "ProcessingCompleted", $"Processing completed for '{photo.FileName}'", completedAt),
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed processing photo {PhotoId}: {FileName}", photoId, photo.FileName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to process photo {PhotoId}", photoId);

            var failedAt = DateTimeOffset.UtcNow;
            photo.MarkFailed(ex.Message, failedAt);
            await photoRepository.UpdateAsync(photo, cancellationToken);
            await eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "ProcessingFailed", $"Processing failed for '{photo.FileName}': {ex.Message}", failedAt),
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

