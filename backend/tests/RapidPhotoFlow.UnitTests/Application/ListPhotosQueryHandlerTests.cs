using FluentAssertions;
using Moq;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Photos.Queries.ListPhotos;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.UnitTests.Application;

public class ListPhotosQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly ListPhotosQueryHandler _handler;

    public ListPhotosQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _handler = new ListPhotosQueryHandler(_photoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoStatusFilter_ShouldReturnAllPhotos()
    {
        // Arrange
        var photos = CreateTestPhotos();
        _photoRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        var query = new ListPhotosQuery(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _photoRepositoryMock.Verify(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        var photos = CreateTestPhotos().Where(p => p.Status == PhotoStatus.Queued).ToList();
        _photoRepositoryMock
            .Setup(r => r.GetAllAsync(PhotoStatus.Queued, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        var query = new ListPhotosQuery("Queued");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be("Queued");
        _photoRepositoryMock.Verify(r => r.GetAllAsync(PhotoStatus.Queued, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidStatusFilter_ShouldReturnAllPhotos()
    {
        // Arrange
        var photos = CreateTestPhotos();
        _photoRepositoryMock
            .Setup(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        var query = new ListPhotosQuery("InvalidStatus");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _photoRepositoryMock.Verify(r => r.GetAllAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static List<Photo> CreateTestPhotos()
    {
        var photo1 = Photo.CreateNew(
            PhotoId.New(),
            "test1.jpg",
            "image/jpeg",
            1024,
            "path/test1.jpg",
            DateTimeOffset.UtcNow);
        photo1.QueueForProcessing();
        photo1.ClearDomainEvents();

        var photo2 = Photo.CreateNew(
            PhotoId.New(),
            "test2.jpg",
            "image/jpeg",
            2048,
            "path/test2.jpg",
            DateTimeOffset.UtcNow);
        photo2.ClearDomainEvents();

        return new List<Photo> { photo1, photo2 };
    }
}

