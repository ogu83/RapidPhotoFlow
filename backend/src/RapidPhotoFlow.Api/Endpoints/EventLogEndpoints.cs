using MediatR;
using RapidPhotoFlow.Application.EventLog.Queries.GetPhotoEventLog;

namespace RapidPhotoFlow.Api.Endpoints;

/// <summary>
/// Minimal API endpoints for event logs.
/// </summary>
public static class EventLogEndpoints
{
    public static IEndpointRouteBuilder MapEventLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/photos")
            .WithTags("Events");

        // Get photo events
        group.MapGet("/{id:guid}/events", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetPhotoEventLogQuery(id);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetPhotoEvents")
        .WithDescription("Get event log for a specific photo");

        return app;
    }
}

