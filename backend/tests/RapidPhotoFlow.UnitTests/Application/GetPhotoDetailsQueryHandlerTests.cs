using FluentAssertions;
using Moq;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Photos.Queries.GetPhotoDetails;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.UnitTests.Application;

public class GetPhotoDetailsQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly GetPhotoDetailsQueryHandler _handler;

    public GetPhotoDetailsQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _handler = new GetPhotoDetailsQueryHandler(_photoRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPhoto_ShouldReturnPhotoDto()
    {
        // Arrange
        var photoId = PhotoId.New();
        var photo = Photo.CreateNew(
            photoId,
            "test.jpg",
            "image/jpeg",
            1024,
            "path/test.jpg",
            DateTimeOffset.UtcNow);

        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var query = new GetPhotoDetailsQuery(photoId.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(photoId.Value);
        result.FileName.Should().Be("test.jpg");
        result.ContentType.Should().Be("image/jpeg");
        result.SizeBytes.Should().Be(1024);
        result.Status.Should().Be("Uploaded");
    }

    [Fact]
    public async Task Handle_NonExistingPhoto_ShouldReturnNull()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        _photoRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<PhotoId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        var query = new GetPhotoDetailsQuery(photoId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

