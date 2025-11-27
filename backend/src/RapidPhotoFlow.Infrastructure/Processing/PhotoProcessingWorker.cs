using System.Diagnostics;
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
        _logger.LogInformation("Photo Processing Worker started and listening for photos");

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
                _logger.LogError(ex, "Unexpected error in photo processing loop");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("Photo Processing Worker stopped");
    }

    private async Task ProcessPhotoAsync(PhotoId photoId, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var scope = _scopeFactory.CreateScope();
        var photoRepository = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();
        var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var photo = await photoRepository.GetByIdAsync(photoId, cancellationToken);

        if (photo is null)
        {
            _logger.LogWarning("Photo not found for processing - PhotoId: {PhotoId}", photoId.Value);
            return;
        }

        if (photo.Status != PhotoStatus.Queued)
        {
            _logger.LogWarning("Photo skipped - not in Queued status - PhotoId: {PhotoId}, FileName: {FileName}, CurrentStatus: {Status}",
                photoId.Value, photo.FileName, photo.Status);
            return;
        }

        _logger.LogInformation("Processing started - PhotoId: {PhotoId}, FileName: {FileName}, Size: {SizeBytes} bytes, ContentType: {ContentType}",
            photoId.Value, photo.FileName, photo.SizeBytes, photo.ContentType);

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

            // Simulate processing with random delay (2-5 seconds)
            var processingDelay = _random.Next(2000, 5000);
            _logger.LogDebug("Simulating processing delay - PhotoId: {PhotoId}, DelayMs: {DelayMs}", photoId.Value, processingDelay);
            await Task.Delay(processingDelay, cancellationToken);

            // Mark as processed
            var completedAt = DateTimeOffset.UtcNow;
            photo.MarkProcessed(completedAt);
            await photoRepository.UpdateAsync(photo, cancellationToken);
            await eventLogRepository.AddAsync(
                EventLogEntry.Create(photoId.Value, "ProcessingCompleted", $"Processing completed for '{photo.FileName}'", completedAt),
                cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("Processing completed successfully - PhotoId: {PhotoId}, FileName: {FileName}, Duration: {DurationMs}ms",
                photoId.Value, photo.FileName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Processing failed - PhotoId: {PhotoId}, FileName: {FileName}, Duration: {DurationMs}ms, Error: {ErrorMessage}",
                photoId.Value, photo.FileName, stopwatch.ElapsedMilliseconds, ex.Message);

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
