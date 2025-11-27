using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Abstractions.Storage;
using RapidPhotoFlow.Application.Photos.Commands.DeletePhoto;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.UnitTests.Application;

public class DeletePhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IEventLogRepository> _eventLogRepositoryMock;
    private readonly Mock<IPhotoStorage> _photoStorageMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<DeletePhotoCommandHandler>> _loggerMock;
    private readonly DeletePhotoCommandHandler _handler;

    public DeletePhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _eventLogRepositoryMock = new Mock<IEventLogRepository>();
        _photoStorageMock = new Mock<IPhotoStorage>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DeletePhotoCommandHandler>>();

        _handler = new DeletePhotoCommandHandler(
            _photoRepositoryMock.Object,
            _eventLogRepositoryMock.Object,
            _photoStorageMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NonExistingPhoto_ShouldReturnNotFound()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<PhotoId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        var command = new DeletePhotoCommand(photoId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Photo not found");
        
        _photoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Photo>(), It.IsAny<CancellationToken>()), Times.Never);
        _photoStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QueuedPhoto_ShouldDeleteFromDatabaseOnly()
    {
        // Arrange
        var photoId = PhotoId.New();
        var photo = Photo.CreateNew(photoId, "test.jpg", "image/jpeg", 1024, "path/test.jpg", DateTimeOffset.UtcNow);
        photo.QueueForProcessing();
        photo.ClearDomainEvents();

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new DeletePhotoCommand(photoId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("test.jpg");

        _eventLogRepositoryMock.Verify(x => x.DeleteByPhotoIdAsync(photoId.Value, It.IsAny<CancellationToken>()), Times.Once);
        _photoRepositoryMock.Verify(x => x.DeleteAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProcessingPhoto_ShouldDeleteFromDatabaseOnly()
    {
        // Arrange
        var photoId = PhotoId.New();
        var photo = Photo.CreateNew(photoId, "test.jpg", "image/jpeg", 1024, "path/test.jpg", DateTimeOffset.UtcNow);
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);
        photo.ClearDomainEvents();

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new DeletePhotoCommand(photoId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        _photoStorageMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProcessedPhoto_ShouldDeleteFromDatabaseAndStorage()
    {
        // Arrange
        var photoId = PhotoId.New();
        var storagePath = "path/test.jpg";
        var photo = Photo.CreateNew(photoId, "test.jpg", "image/jpeg", 1024, storagePath, DateTimeOffset.UtcNow);
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);
        photo.MarkProcessed(DateTimeOffset.UtcNow);
        photo.ClearDomainEvents();

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new DeletePhotoCommand(photoId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("test.jpg");

        _eventLogRepositoryMock.Verify(x => x.DeleteByPhotoIdAsync(photoId.Value, It.IsAny<CancellationToken>()), Times.Once);
        _photoRepositoryMock.Verify(x => x.DeleteAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
        _photoStorageMock.Verify(x => x.DeleteAsync(storagePath, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FailedPhoto_ShouldDeleteFromDatabaseAndStorage()
    {
        // Arrange
        var photoId = PhotoId.New();
        var storagePath = "path/test.jpg";
        var photo = Photo.CreateNew(photoId, "test.jpg", "image/jpeg", 1024, storagePath, DateTimeOffset.UtcNow);
        photo.QueueForProcessing();
        photo.MarkFailed("Some error", DateTimeOffset.UtcNow);
        photo.ClearDomainEvents();

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new DeletePhotoCommand(photoId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        _photoStorageMock.Verify(x => x.DeleteAsync(storagePath, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_StorageDeleteFails_ShouldStillDeleteFromDatabase()
    {
        // Arrange
        var photoId = PhotoId.New();
        var storagePath = "path/test.jpg";
        var photo = Photo.CreateNew(photoId, "test.jpg", "image/jpeg", 1024, storagePath, DateTimeOffset.UtcNow);
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);
        photo.MarkProcessed(DateTimeOffset.UtcNow);
        photo.ClearDomainEvents();

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoStorageMock
            .Setup(x => x.DeleteAsync(storagePath, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("File locked"));

        var command = new DeletePhotoCommand(photoId.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        _photoRepositoryMock.Verify(x => x.DeleteAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

