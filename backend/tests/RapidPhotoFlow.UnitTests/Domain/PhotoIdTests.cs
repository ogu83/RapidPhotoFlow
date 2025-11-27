using FluentAssertions;
using RapidPhotoFlow.Domain.Photos;

namespace RapidPhotoFlow.UnitTests.Domain;

public class PhotoIdTests
{
    [Fact]
    public void New_ShouldCreateUniqueId()
    {
        // Act
        var id1 = PhotoId.New();
        var id2 = PhotoId.New();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void TryParse_ValidGuid_ShouldReturnTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var result = PhotoId.TryParse(guidString, out var photoId);

        // Assert
        result.Should().BeTrue();
        photoId.Value.Should().Be(guid);
    }

    [Fact]
    public void TryParse_InvalidGuid_ShouldReturnFalse()
    {
        // Arrange
        var invalidGuid = "not-a-guid";

        // Act
        var result = PhotoId.TryParse(invalidGuid, out var photoId);

        // Assert
        result.Should().BeFalse();
        photoId.Should().Be(default(PhotoId));
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var photoId = new PhotoId(guid);

        // Act
        var result = photoId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var photoId = new PhotoId(guid);

        // Act
        Guid result = photoId;

        // Assert
        result.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        PhotoId photoId = guid;

        // Assert
        photoId.Value.Should().Be(guid);
    }
}

