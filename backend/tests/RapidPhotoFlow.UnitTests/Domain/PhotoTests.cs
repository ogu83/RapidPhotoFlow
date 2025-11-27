using FluentAssertions;
using RapidPhotoFlow.Domain.Photos;
using RapidPhotoFlow.Domain.Photos.Events;

namespace RapidPhotoFlow.UnitTests.Domain;

public class PhotoTests
{
    [Fact]
    public void CreateNew_ShouldCreatePhotoWithUploadedStatus()
    {
        // Arrange
        var id = PhotoId.New();
        var fileName = "test.jpg";
        var contentType = "image/jpeg";
        var sizeBytes = 1024L;
        var storagePath = "path/to/file.jpg";
        var uploadedAt = DateTimeOffset.UtcNow;

        // Act
        var photo = Photo.CreateNew(id, fileName, contentType, sizeBytes, storagePath, uploadedAt);

        // Assert
        photo.Id.Should().Be(id);
        photo.FileName.Should().Be(fileName);
        photo.ContentType.Should().Be(contentType);
        photo.SizeBytes.Should().Be(sizeBytes);
        photo.StoragePath.Should().Be(storagePath);
        photo.Status.Should().Be(PhotoStatus.Uploaded);
        photo.UploadedAt.Should().Be(uploadedAt);
        photo.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PhotoUploadedDomainEvent>();
    }

    [Fact]
    public void QueueForProcessing_FromUploaded_ShouldSetStatusToQueued()
    {
        // Arrange
        var photo = CreateTestPhoto();

        // Act
        photo.QueueForProcessing();

        // Assert
        photo.Status.Should().Be(PhotoStatus.Queued);
        photo.DomainEvents.Should().HaveCount(2);
        photo.DomainEvents.Last().Should().BeOfType<PhotoQueuedForProcessingDomainEvent>();
    }

    [Fact]
    public void QueueForProcessing_FromNonUploadedStatus_ShouldThrow()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);

        // Act
        var action = () => photo.QueueForProcessing();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot queue photo from status*");
    }

    [Fact]
    public void StartProcessing_FromQueued_ShouldSetStatusToProcessing()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();
        var startTime = DateTimeOffset.UtcNow;

        // Act
        photo.StartProcessing(startTime);

        // Assert
        photo.Status.Should().Be(PhotoStatus.Processing);
        photo.ProcessingStartedAt.Should().Be(startTime);
        photo.DomainEvents.Should().HaveCount(3);
        photo.DomainEvents.Last().Should().BeOfType<PhotoProcessingStartedDomainEvent>();
    }

    [Fact]
    public void StartProcessing_FromNonQueuedStatus_ShouldThrow()
    {
        // Arrange
        var photo = CreateTestPhoto();

        // Act
        var action = () => photo.StartProcessing(DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot start processing from status*");
    }

    [Fact]
    public void MarkProcessed_FromProcessing_ShouldSetStatusToProcessed()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);
        var completedAt = DateTimeOffset.UtcNow;

        // Act
        photo.MarkProcessed(completedAt);

        // Assert
        photo.Status.Should().Be(PhotoStatus.Processed);
        photo.ProcessingCompletedAt.Should().Be(completedAt);
        photo.ErrorMessage.Should().BeNull();
        photo.DomainEvents.Should().HaveCount(4);
        photo.DomainEvents.Last().Should().BeOfType<PhotoProcessingCompletedDomainEvent>();
    }

    [Fact]
    public void MarkProcessed_FromNonProcessingStatus_ShouldThrow()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();

        // Act
        var action = () => photo.MarkProcessed(DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot complete processing from status*");
    }

    [Fact]
    public void MarkFailed_FromQueued_ShouldSetStatusToFailed()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();
        var failedAt = DateTimeOffset.UtcNow;
        var errorMessage = "Test error";

        // Act
        photo.MarkFailed(errorMessage, failedAt);

        // Assert
        photo.Status.Should().Be(PhotoStatus.Failed);
        photo.ProcessingCompletedAt.Should().Be(failedAt);
        photo.ErrorMessage.Should().Be(errorMessage);
        photo.DomainEvents.Should().HaveCount(3);
        photo.DomainEvents.Last().Should().BeOfType<PhotoProcessingFailedDomainEvent>();
    }

    [Fact]
    public void MarkFailed_FromProcessing_ShouldSetStatusToFailed()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();
        photo.StartProcessing(DateTimeOffset.UtcNow);
        var failedAt = DateTimeOffset.UtcNow;
        var errorMessage = "Test error";

        // Act
        photo.MarkFailed(errorMessage, failedAt);

        // Assert
        photo.Status.Should().Be(PhotoStatus.Failed);
        photo.ErrorMessage.Should().Be(errorMessage);
        photo.DomainEvents.Last().Should().BeOfType<PhotoProcessingFailedDomainEvent>();
    }

    [Fact]
    public void MarkFailed_FromNonQueuedOrProcessingStatus_ShouldThrow()
    {
        // Arrange
        var photo = CreateTestPhoto();

        // Act
        var action = () => photo.MarkFailed("error", DateTimeOffset.UtcNow);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot mark failed from status*");
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var photo = CreateTestPhoto();
        photo.QueueForProcessing();

        // Act
        photo.ClearDomainEvents();

        // Assert
        photo.DomainEvents.Should().BeEmpty();
    }

    private static Photo CreateTestPhoto()
    {
        return Photo.CreateNew(
            PhotoId.New(),
            "test.jpg",
            "image/jpeg",
            1024,
            "path/test.jpg",
            DateTimeOffset.UtcNow);
    }
}

