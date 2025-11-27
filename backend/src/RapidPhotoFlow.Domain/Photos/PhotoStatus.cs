namespace RapidPhotoFlow.Domain.Photos;

/// <summary>
/// Represents the processing status of a photo.
/// </summary>
public enum PhotoStatus
{
    Uploaded = 0,
    Queued = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}

