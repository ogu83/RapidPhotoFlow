namespace RapidPhotoFlow.Application.Photos.Dtos;

/// <summary>
/// Lightweight photo DTO for list views.
/// </summary>
public sealed record PhotoListItemDto(
    Guid Id,
    string FileName,
    string Status,
    DateTimeOffset UploadedAt);

