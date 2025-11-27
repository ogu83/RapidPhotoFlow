namespace RapidPhotoFlow.Application.Abstractions.Storage;

/// <summary>
/// Interface for photo file storage operations.
/// </summary>
public interface IPhotoStorage
{
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);
    Task<Stream?> GetAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}

