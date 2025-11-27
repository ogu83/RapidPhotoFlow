
# RapidPhotoFlow Backend Design (.NET 9, Clean Architecture, CQRS)

This document captures the backend design for **RapidPhotoFlow**, aligned with the TeamFront hackathon brief, CQRS & Clean Architecture guidelines, and RLS patterns for future multi-tenant scenarios.

---

## 1. Architecture Overview

### 1.1 Layers & Projects

We use a classic Clean Architecture split:

- **RapidPhotoFlow.Domain**
  - Aggregates: `Photo`, `UploadBatch`, `EventLogEntry`
  - Value objects: `PhotoId`, `UploadBatchId`, `StoragePath`
  - Enums: `PhotoStatus`
  - Domain events: `PhotoUploadedDomainEvent`, `PhotoProcessingStartedDomainEvent`, etc.
- **RapidPhotoFlow.Application**
  - CQRS commands & queries
  - DTOs
  - Interfaces: repositories, storage, queues, notification services
  - Pipeline behaviors (validation, logging, tenant injection)
- **RapidPhotoFlow.Infrastructure**
  - EF Core DbContext, migrations, repositories
  - File storage implementation
  - Processing queue + background worker
  - SignalR notification implementation (optional)
  - DB session tenant injection & RLS plumbing
- **RapidPhotoFlow.Api**
  - Minimal API endpoints (REST)
  - Tenant middleware
  - DI bootstrap (AddApplication, AddInfrastructure)
  - SignalR Hub endpoints (optional)

Dependencies:

- `Domain` has **no** project references.
- `Application` references `Domain`.
- `Infrastructure` references `Application` + `Domain`.
- `Api` references `Application` + `Infrastructure`.

### 1.2 Key Concepts

- **CQRS**: Commands mutate state; queries read state only.
- **Domain Model**: Aggregate `Photo` encapsulates status transitions and business rules.
- **Domain Events**: Status changes raise events used to write workflow logs and drive processing.
- **Background Processing**: An in-memory queue plus `BackgroundService` simulates async processing.
- **Realtime** (optional): `IPhotoNotificationService` abstraction, with SignalR-based implementation.

---

## 2. Solution Structure & Folder Layout

Recommended folder structure (backend only):

```text
RapidPhotoFlow/
  backend/
    RapidPhotoFlow.sln

    src/
      RapidPhotoFlow.Domain/
        Photos/
          Photo.cs
          PhotoId.cs
          PhotoStatus.cs
          Events/
            PhotoUploadedDomainEvent.cs
            PhotoQueuedForProcessingDomainEvent.cs
            PhotoProcessingStartedDomainEvent.cs
            PhotoProcessingCompletedDomainEvent.cs
            PhotoProcessingFailedDomainEvent.cs
        Batches/
          UploadBatch.cs
          UploadBatchId.cs
        EventLog/
          EventLogEntry.cs
        Common/
          Abstractions/
            IDomainEvent.cs
          ValueObjects/
            StoragePath.cs

      RapidPhotoFlow.Application/
        Abstractions/
          Persistence/
            IPhotoRepository.cs
            IUploadBatchRepository.cs
            IEventLogRepository.cs
          Storage/
            IPhotoStorage.cs
          Processing/
            IPhotoProcessingQueue.cs
            IPhotoProcessingStrategy.cs
          Notifications/
            IPhotoNotificationService.cs
          Tenancy/
            ITenantContext.cs
        Photos/
          Commands/
            UploadPhotos/
              UploadPhotosCommand.cs
              UploadPhotosCommandHandler.cs
            StartPhotoProcessing/
              StartPhotoProcessingCommand.cs
              StartPhotoProcessingCommandHandler.cs
            CompletePhotoProcessing/
              CompletePhotoProcessingCommand.cs
              CompletePhotoProcessingCommandHandler.cs
            FailPhotoProcessing/
              FailPhotoProcessingCommand.cs
              FailPhotoProcessingCommandHandler.cs
          Queries/
            GetPhotoDetails/
              GetPhotoDetailsQuery.cs
              GetPhotoDetailsQueryHandler.cs
            ListPhotos/
              ListPhotosQuery.cs
              ListPhotosQueryHandler.cs
          Dtos/
            PhotoDto.cs
            PhotoListItemDto.cs
        Batches/
          Queries/
            GetBatchDetails/
            ListBatches/
        EventLog/
          Queries/
            GetPhotoEventLog/
        Common/
          Behaviors/
            ValidationBehavior.cs
            LoggingBehavior.cs
            TenantInjectionBehavior.cs

      RapidPhotoFlow.Infrastructure/
        Persistence/
          ApplicationDbContext.cs
          Configurations/
            PhotoConfiguration.cs
            UploadBatchConfiguration.cs
            EventLogEntryConfiguration.cs
          Migrations/
          Repositories/
            PhotoRepository.cs
            UploadBatchRepository.cs
            EventLogRepository.cs
          Tenancy/
            TenantDbSessionInterceptor.cs
        Storage/
          LocalFilePhotoStorage.cs
        Processing/
          InMemoryPhotoProcessingQueue.cs
          PhotoProcessingWorker.cs
          RandomDelayPhotoProcessingStrategy.cs
        Notifications/
          SignalRPhotoNotificationService.cs
          NoOpPhotoNotificationService.cs
        DependencyInjection/
          ServiceCollectionExtensions.cs

      RapidPhotoFlow.Api/
        Program.cs
        Endpoints/
          PhotosEndpoints.cs
          BatchesEndpoints.cs
          EventLogEndpoints.cs
        Hubs/
          PhotoFlowHub.cs
        Middleware/
          TenantResolutionMiddleware.cs
```

This layout keeps features grouped (Photos, Batches, EventLog) while preserving Clean Architecture boundaries and CQRS separation.

---

## 3. .csproj Layout

### 3.1 `RapidPhotoFlow.Domain/RapidPhotoFlow.Domain.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

No external package references; pure domain model.

---

### 3.2 `RapidPhotoFlow.Application/RapidPhotoFlow.Application.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\RapidPhotoFlow.Domain\RapidPhotoFlow.Domain.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Set versions via `dotnet add package` in Cursor -->
    <PackageReference Include="MediatR" Version="x.y.z" />
    <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="x.y.z" />
    <PackageReference Include="FluentValidation" Version="x.y.z" />
  </ItemGroup>

</Project>
```

---

### 3.3 `RapidPhotoFlow.Infrastructure/RapidPhotoFlow.Infrastructure.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\RapidPhotoFlow.Domain\RapidPhotoFlow.Domain.csproj" />
    <ProjectReference Include="..\RapidPhotoFlow.Application\RapidPhotoFlow.Application.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- EF Core & provider (SQL Server / PostgreSQL) -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="x.y.z" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="x.y.z" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="x.y.z">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>

    <!-- For background services & DI extensions -->
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="x.y.z" />
  </ItemGroup>

</Project>
```

You can swap in `Npgsql.EntityFrameworkCore.PostgreSQL` or `Microsoft.EntityFrameworkCore.SqlServer` depending on your DB.

---

### 3.4 `RapidPhotoFlow.Api/RapidPhotoFlow.Api.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\RapidPhotoFlow.Application\RapidPhotoFlow.Application.csproj" />
    <ProjectReference Include="..\RapidPhotoFlow.Infrastructure\RapidPhotoFlow.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Minimal APIs, Swagger, SignalR -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="x.y.z" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Core" Version="x.y.z" />
  </ItemGroup>

</Project>
```

In practice you’ll add packages with `dotnet add package`, and Cursor can fill in the actual versions.

---

## 4. Sample Domain Model & CQRS Handlers

Below are minimal but realistic samples you can paste into Cursor and build on.

### 4.1 Domain: `PhotoStatus` enum

```csharp
namespace RapidPhotoFlow.Domain.Photos;

public enum PhotoStatus
{
    Uploaded = 0,
    Queued = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4
}
```

### 4.2 Domain: `PhotoId` value object

```csharp
using System.Diagnostics.CodeAnalysis;

namespace RapidPhotoFlow.Domain.Photos;

public readonly record struct PhotoId(Guid Value)
{
    public static PhotoId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static bool TryParse(string value, [NotNullWhen(true)] out PhotoId id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = new PhotoId(guid);
            return true;
        }

        id = default;
        return false;
    }
}
```

### 4.3 Domain: `Photo` aggregate

```csharp
using RapidPhotoFlow.Domain.Common.Abstractions;

namespace RapidPhotoFlow.Domain.Photos;

public sealed class Photo
{
    private readonly List<IDomainEvent> _domainEvents = new();

    private Photo() { } // For EF

    public PhotoId Id { get; private set; }
    public string TenantId { get; private set; } = null!;
    public string? UploadBatchId { get; private set; }

    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }

    public string StoragePath { get; private set; } = null!;
    public PhotoStatus Status { get; private set; }

    public DateTimeOffset UploadedAt { get; private set; }
    public DateTimeOffset? ProcessingStartedAt { get; private set; }
    public DateTimeOffset? ProcessingCompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Photo CreateNew(
        PhotoId id,
        string tenantId,
        string? uploadBatchId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storagePath,
        DateTimeOffset uploadedAtUtc)
    {
        var photo = new Photo
        {
            Id = id,
            TenantId = tenantId,
            UploadBatchId = uploadBatchId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            StoragePath = storagePath,
            Status = PhotoStatus.Uploaded,
            UploadedAt = uploadedAtUtc
        };

        photo.AddDomainEvent(new PhotoUploadedDomainEvent(photo.Id));
        return photo;
    }

    public void QueueForProcessing()
    {
        if (Status != PhotoStatus.Uploaded)
        {
            throw new InvalidOperationException($"Cannot queue photo from status {Status}.");
        }

        Status = PhotoStatus.Queued;
        AddDomainEvent(new PhotoQueuedForProcessingDomainEvent(Id));
    }

    public void StartProcessing(DateTimeOffset startedAtUtc)
    {
        if (Status is not PhotoStatus.Queued)
        {
            throw new InvalidOperationException($"Cannot start processing from status {Status}.");
        }

        Status = PhotoStatus.Processing;
        ProcessingStartedAt = startedAtUtc;
        AddDomainEvent(new PhotoProcessingStartedDomainEvent(Id));
    }

    public void MarkProcessed(DateTimeOffset completedAtUtc)
    {
        if (Status is not PhotoStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot complete processing from status {Status}.");
        }

        Status = PhotoStatus.Processed;
        ProcessingCompletedAt = completedAtUtc;
        ErrorMessage = null;

        AddDomainEvent(new PhotoProcessingCompletedDomainEvent(Id));
    }

    public void MarkFailed(string errorMessage, DateTimeOffset failedAtUtc)
    {
        if (Status is not PhotoStatus.Queued and not PhotoStatus.Processing)
        {
            throw new InvalidOperationException($"Cannot mark failed from status {Status}.");
        }

        Status = PhotoStatus.Failed;
        ProcessingCompletedAt = failedAtUtc;
        ErrorMessage = errorMessage;

        AddDomainEvent(new PhotoProcessingFailedDomainEvent(Id, errorMessage));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
}
```

You would also define the corresponding `Photo*DomainEvent` records implementing `IDomainEvent` in the same namespace.

---

### 4.4 Application: DTOs

```csharp
namespace RapidPhotoFlow.Application.Photos.Dtos;

public sealed record PhotoDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    DateTimeOffset UploadedAt,
    DateTimeOffset? ProcessingStartedAt,
    DateTimeOffset? ProcessingCompletedAt,
    string? ErrorMessage);
```

A simple projection DTO used by queries and command responses.

---

### 4.5 Application: `UploadPhotosCommand`

We keep the command free of tenantId (RLS pattern) and avoid using web-specific types like `IFormFile` in the Application layer.

```csharp
using MediatR;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

public sealed record UploadPhotosCommand(
    IReadOnlyCollection<UploadPhotoItem> Photos)
    : IRequest<IReadOnlyCollection<PhotoDto>>;
```

`UploadPhotoItem` is a simple descriptor for the uploaded files:

```csharp
namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

public sealed record UploadPhotoItem(
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);
```

> Note: The API layer will adapt `IFormFile` to `UploadPhotoItem` before sending the command.

---

### 4.6 Application: `IPhotoUploadService`

```csharp
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

public interface IPhotoUploadService
{
    Task<IReadOnlyCollection<PhotoDto>> UploadAsync(
        IReadOnlyCollection<UploadPhotoItem> files,
        CancellationToken cancellationToken);
}
```

Implementation lives in Infrastructure and uses `ITenantContext`, `IPhotoStorage`, and `IPhotoRepository`.

---

### 4.7 Application: `UploadPhotosCommandHandler`

A thin CQRS handler orchestrating the upload service.

```csharp
using MediatR;

namespace RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

public sealed class UploadPhotosCommandHandler
    : IRequestHandler<UploadPhotosCommand, IReadOnlyCollection<PhotoDto>>
{
    private readonly IPhotoUploadService _uploadService;

    public UploadPhotosCommandHandler(IPhotoUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    public async Task<IReadOnlyCollection<PhotoDto>> Handle(
        UploadPhotosCommand request,
        CancellationToken cancellationToken)
    {
        // All heavy lifting is delegated to the upload service.
        return await _uploadService.UploadAsync(request.Photos, cancellationToken);
    }
}
```

This matches the AGENTS CQRS rule that handlers must be small, orchestration-only, and testable.

---

### 4.8 Application: Sample Query – `ListPhotosQuery`

```csharp
using MediatR;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Queries.ListPhotos;

public sealed record ListPhotosQuery(
    string? Status,
    int Page,
    int PageSize) : IRequest<IReadOnlyCollection<PhotoListItemDto>>;
```

`PhotoListItemDto` can be a slim projection subset of `PhotoDto`.

Handler:

```csharp
using MediatR;
using RapidPhotoFlow.Application.Abstractions.Persistence;
using RapidPhotoFlow.Application.Photos.Dtos;

namespace RapidPhotoFlow.Application.Photos.Queries.ListPhotos;

public sealed class ListPhotosQueryHandler
    : IRequestHandler<ListPhotosQuery, IReadOnlyCollection<PhotoListItemDto>>
{
    private readonly IPhotoReadRepository _photoReadRepository;

    public ListPhotosQueryHandler(IPhotoReadRepository photoReadRepository)
    {
        _photoReadRepository = photoReadRepository;
    }

    public async Task<IReadOnlyCollection<PhotoListItemDto>> Handle(
        ListPhotosQuery request,
        CancellationToken cancellationToken)
    {
        return await _photoReadRepository.ListAsync(
            status: request.Status,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken);
    }
}
```

Here `IPhotoReadRepository` is a read-optimized repository interface implemented in Infrastructure.

---

### 4.9 API: Minimal Endpoint Example (for reference)

In `PhotosEndpoints.cs`:

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RapidPhotoFlow.Application.Photos.Commands.UploadPhotos;

public static class PhotosEndpoints
{
    public static IEndpointRouteBuilder MapPhotosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/photos");

        group.MapPost("/", async (
            HttpRequest request,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest("Expected multipart/form-data");
            }

            var form = await request.ReadFormAsync(ct);

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
        });

        return app;
    }
}
```

The endpoint:
- Adapts `IFormFile` → `UploadPhotoItem`
- Sends `UploadPhotosCommand`
- Returns the created `PhotoDto` list

This is enough to get the initial vertical slice running in Cursor.

---

You can now:

1. Drop this Markdown file into your repo as `backend/docs/RapidPhotoFlow_Backend_Design.md`.
2. Use the solution structure, `.csproj` skeletons, and sample code as prompts/snippets in Cursor to auto-generate the rest of the backend.
