using RapidPhotoFlow.Application.Abstractions.Storage;

namespace RapidPhotoFlow.Infrastructure.Storage;

/// <summary>
/// Local file system implementation of photo storage.
/// </summary>
public class LocalFilePhotoStorage : IPhotoStorage
{
    private readonly string _basePath;

    public LocalFilePhotoStorage(string basePath)
    {
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        return uniqueFileName;
    }

    public Task<Stream?> GetAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);

        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, storagePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}

