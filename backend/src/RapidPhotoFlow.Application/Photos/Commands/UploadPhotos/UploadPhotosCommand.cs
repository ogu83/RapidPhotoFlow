using MediatR;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

/// <summary>
/// Command to upload one or more photos.
/// </summary>
public sealed record UploadPhotosCommand(
    IReadOnlyCollection<UploadPhotoItem> Photos) : IRequest<IReadOnlyCollection<PhotoDto>>;

