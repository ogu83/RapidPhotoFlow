namespace RapidPhotoFlow.Application.Photos.Dtos;

/// <summary>
/// Full photo details DTO.
/// </summary>
public sealed record PhotoDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    string StoragePath,
    DateTimeOffset UploadedAt,
    DateTimeOffset? ProcessingStartedAt,
    DateTimeOffset? ProcessingCompletedAt,
    string? ErrorMessage);

