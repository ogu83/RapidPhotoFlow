using MediatR;
using RapidPhotoFlow.Application.EventLog.Dtos;

namespace RapidPhotoFlow.Application.EventLog.Queries.GetPhotoEventLog;

/// <summary>
/// Query to get event log entries for a photo.
/// </summary>
public sealed record GetPhotoEventLogQuery(Guid PhotoId) : IRequest<IReadOnlyCollection<PhotoEventDto>>;

