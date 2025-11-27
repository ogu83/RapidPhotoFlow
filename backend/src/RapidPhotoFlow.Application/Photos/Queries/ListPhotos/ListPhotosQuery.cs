using MediatR;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Queries.ListPhotos;

/// <summary>
/// Query to list photos with optional status filter.
/// </summary>
public sealed record ListPhotosQuery(string? Status) : IRequest<IReadOnlyCollection<PhotoListItemDto>>;

