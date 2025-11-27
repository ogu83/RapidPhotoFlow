using MediatR;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Photos.Dtos;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Photos.Queries.ListPhotos;

/// <summary>
/// Handler for ListPhotosQuery.
/// </summary>
public sealed class ListPhotosQueryHandler : IRequestHandler<ListPhotosQuery, IReadOnlyCollection<PhotoListItemDto>>
{
    private readonly IPhotoRepository _photoRepository;

    public ListPhotosQueryHandler(IPhotoRepository photoRepository)
    {
        _photoRepository = photoRepository;
    }

    public async Task<IReadOnlyCollection<PhotoListItemDto>> Handle(
        ListPhotosQuery request,
        CancellationToken cancellationToken)
    {
        PhotoStatus? statusFilter = null;

        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<PhotoStatus>(request.Status, ignoreCase: true, out var parsed))
        {
            statusFilter = parsed;
        }

        var photos = await _photoRepository.GetAllAsync(statusFilter, cancellationToken);

        return photos
            .Select(p => new PhotoListItemDto(
                p.Id.Value,
                p.FileName,
                p.Status.ToString(),
                p.UploadedAt))
            .ToList();
    }
}

