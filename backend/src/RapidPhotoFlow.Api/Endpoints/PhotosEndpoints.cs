using MediatR;
using RapidPhotoFlow.Application.Abstractions.Storage;
using RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;
using RapidPhotoFlow.Application.Photos.Queries.GetPhotoDetails;
using RapidPhotoFlow.Application.Photos.Queries.ListPhotos;

namespace RapidPhotoFlow.Api.Endpoints;

/// <summary>
/// Minimal API endpoints for photos.
/// </summary>
public static class PhotosEndpoints
{
    public static IEndpointRouteBuilder MapPhotosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/photos")
            .WithTags("Photos");

        // Upload photos
        group.MapPost("/", async (HttpRequest request, IMediator mediator, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Expected multipart/form-data");
            }

            var form = await request.ReadFormAsync(ct);

            if (form.Files.Count == 0)
            {
                return Results.BadRequest("No files uploaded");
            }

            var files = form.Files
                .Select(file => new UploadPhotoItem(
                    FileName: file.FileName,
                    ContentType: file.ContentType,
                    SizeBytes: file.Length,
                    Content: file.OpenReadStream()))
                .ToList();

            var command = new UploadPhotosCommand(files);
            var result = await mediator.Send(command, ct);

            return Results.Created("/api/photos", result);
        })
        .WithName("UploadPhotos")
        .WithDescription("Upload one or more photos")
        .Accepts<IFormFileCollection>("multipart/form-data")
        .DisableAntiforgery();

        // List photos
        group.MapGet("/", async (string? status, IMediator mediator, CancellationToken ct) =>
        {
            var query = new ListPhotosQuery(status);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("ListPhotos")
        .WithDescription("List all photos with optional status filter");

        // Get photo details
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetPhotoDetailsQuery(id);
            var result = await mediator.Send(query, ct);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        })
        .WithName("GetPhoto")
        .WithDescription("Get photo details by ID");

        // Get photo file (image)
        group.MapGet("/{id:guid}/file", async (Guid id, IMediator mediator, IPhotoStorage photoStorage, CancellationToken ct) =>
        {
            var query = new GetPhotoDetailsQuery(id);
            var photo = await mediator.Send(query, ct);

            if (photo is null)
            {
                return Results.NotFound();
            }

            var stream = await photoStorage.GetAsync(photo.StoragePath, ct);
            
            if (stream is null)
            {
                return Results.NotFound("Photo file not found");
            }

            return Results.File(stream, photo.ContentType, photo.FileName);
        })
        .WithName("GetPhotoFile")
        .WithDescription("Get the actual photo file/image");

        return app;
    }
}
