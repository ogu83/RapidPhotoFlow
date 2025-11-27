using MediatR;

namespace RapidPhotoFlow.Application.Photos.Commands.DeletePhoto;

/// <summary>
/// Command to delete a photo.
/// </summary>
public sealed record DeletePhotoCommand(Guid PhotoId) : IRequest<DeletePhotoResult>;

/// <summary>
/// Result of the delete photo operation.
/// </summary>
public sealed record DeletePhotoResult(bool Success, string Message);

