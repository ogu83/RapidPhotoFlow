namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

/// <summary>
/// Represents a single photo upload item.
/// </summary>
public sealed record UploadPhotoItem(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

