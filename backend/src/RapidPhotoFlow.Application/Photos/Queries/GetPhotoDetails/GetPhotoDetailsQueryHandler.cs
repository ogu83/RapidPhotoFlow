using MediatR;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Photos.Dtos;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.Application.Photos.Queries.GetPhotoDetails;

/// <summary>
/// Handler for GetPhotoDetailsQuery.
/// </summary>
public sealed class GetPhotoDetailsQueryHandler : IRequestHandler<GetPhotoDetailsQuery, PhotoDto?>
{
    private readonly IPhotoRepository _photoRepository;

    public GetPhotoDetailsQueryHandler(IPhotoRepository photoRepository)
    {
        _photoRepository = photoRepository;
    }

    public async Task<PhotoDto?> Handle(
        GetPhotoDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(
            new PhotoId(request.PhotoId),
            cancellationToken);

        if (photo is null)
            return null;

        return new PhotoDto(
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
}

