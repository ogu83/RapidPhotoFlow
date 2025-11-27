using System.Threading.Channels;
using RapidPhotoFlow.Application.Abstractions.Processing;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Infrastructure.Processing;

/// <summary>
/// In-memory implementation of photo processing queue using Channels.
/// </summary>
public class InMemoryPhotoProcessingQueue : IPhotoProcessingQueue
{
    private readonly Channel<PhotoId> _channel;

    public InMemoryPhotoProcessingQueue()
    {
        _channel = Channel.CreateUnbounded<PhotoId>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public async Task EnqueueAsync(PhotoId photoId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(photoId, cancellationToken);
    }

    public async Task<PhotoId?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out var photoId))
                {
                    return photoId;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested
        }

        return null;
    }
}

