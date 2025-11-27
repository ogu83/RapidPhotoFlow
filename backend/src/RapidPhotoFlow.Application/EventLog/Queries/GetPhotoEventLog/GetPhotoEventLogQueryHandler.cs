using MediatR;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.EventLog.Dtos;

namespace RapidPhotoFlow.Application.EventLog.Queries.GetPhotoEventLog;

/// <summary>
/// Handler for GetPhotoEventLogQuery.
/// </summary>
public sealed class GetPhotoEventLogQueryHandler : IRequestHandler<GetPhotoEventLogQuery, IReadOnlyCollection<PhotoEventDto>>
{
    private readonly IEventLogRepository _eventLogRepository;

    public GetPhotoEventLogQueryHandler(IEventLogRepository eventLogRepository)
    {
        _eventLogRepository = eventLogRepository;
    }

    public async Task<IReadOnlyCollection<PhotoEventDto>> Handle(
        GetPhotoEventLogQuery request,
        CancellationToken cancellationToken)
    {
        var entries = await _eventLogRepository.GetByPhotoIdAsync(request.PhotoId, cancellationToken);

        return entries
            .Select(e => new PhotoEventDto(
                e.Id,
                e.PhotoId,
                e.EventType,
                e.Message,
                e.CreatedAt,
                e.MetadataJson))
            .OrderBy(e => e.CreatedAt)
            .ToList();
    }
}

