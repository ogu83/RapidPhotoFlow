using FluentAssertions;
using Moq;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.EventLog.Queries.GetPhotoEventLog;
using RapidPhotoFlow.Domain.EventLog;

namespace RapidPhotoFlow.UnitTests.Application;

public class GetPhotoEventLogQueryHandlerTests
{
    private readonly Mock<IEventLogRepository> _eventLogRepositoryMock;
    private readonly GetPhotoEventLogQueryHandler _handler;

    public GetPhotoEventLogQueryHandlerTests()
    {
        _eventLogRepositoryMock = new Mock<IEventLogRepository>();
        _handler = new GetPhotoEventLogQueryHandler(_eventLogRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingEvents_ShouldReturnOrderedEvents()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var events = new List<EventLogEntry>
        {
            EventLogEntry.Create(photoId, "ProcessingStarted", "Processing started", now.AddMinutes(1)),
            EventLogEntry.Create(photoId, "Uploaded", "Photo uploaded", now),
            EventLogEntry.Create(photoId, "ProcessingCompleted", "Processing completed", now.AddMinutes(2))
        };

        _eventLogRepositoryMock
            .Setup(r => r.GetByPhotoIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPhotoEventLogQuery(photoId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.First().EventType.Should().Be("Uploaded");
        result.Last().EventType.Should().Be("ProcessingCompleted");
    }

    [Fact]
    public async Task Handle_NoEvents_ShouldReturnEmptyList()
    {
        // Arrange
        var photoId = Guid.NewGuid();
        _eventLogRepositoryMock
            .Setup(r => r.GetByPhotoIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EventLogEntry>());

        var query = new GetPhotoEventLogQuery(photoId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

