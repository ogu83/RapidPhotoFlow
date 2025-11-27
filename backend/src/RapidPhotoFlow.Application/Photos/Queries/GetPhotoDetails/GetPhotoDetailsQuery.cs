using MediatR;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Queries.GetPhotoDetails;

/// <summary>
/// Query to get full photo details.
/// </summary>
public sealed record GetPhotoDetailsQuery(Guid PhotoId) : IRequest<PhotoDto?>;

