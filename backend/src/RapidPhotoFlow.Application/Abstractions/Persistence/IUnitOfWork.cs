namespace RapidPhotoFlow.Application.Abstractions.Persistence;

/// <summary>
/// Unit of Work pattern interface for transactional operations.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

