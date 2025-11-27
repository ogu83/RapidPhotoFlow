# RapidPhotoFlow Backend Documentation

## Overview

RapidPhotoFlow is a photo upload, processing, and review workflow application built with .NET 9, following Clean Architecture and CQRS design patterns.

## Architecture

### Solution Structure

```
backend/
├── RapidPhotoFlow.sln
├── src/
│   ├── RapidPhotoFlow.Domain/        # Domain entities, value objects, events
│   ├── RapidPhotoFlow.Application/   # CQRS commands, queries, interfaces
│   ├── RapidPhotoFlow.Infrastructure/# EF Core, repositories, processing
│   └── RapidPhotoFlow.Api/           # Minimal API endpoints
├── tests/
│   └── RapidPhotoFlow.UnitTests/     # Unit tests
└── Docs/
    └── README.md                      # This file
```

### Layer Dependencies

- **Domain** → No dependencies (pure domain model)
- **Application** → Domain
- **Infrastructure** → Application, Domain
- **Api** → Application, Infrastructure

## Key Technologies

- **.NET 9.0** - Framework
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - Database (via Npgsql)
- **MediatR** - CQRS pattern implementation
- **Swashbuckle** - Swagger/OpenAPI documentation

## Domain Model

### Photo Aggregate

The `Photo` entity is the core aggregate with the following status transitions:

```
Uploaded → Queued → Processing → Processed
                              ↘ Failed
```

### Status Values

- `Uploaded` - Photo has been uploaded
- `Queued` - Photo is queued for processing
- `Processing` - Photo is being processed
- `Processed` - Processing completed successfully
- `Failed` - Processing failed

### Domain Events

- `PhotoUploadedDomainEvent`
- `PhotoQueuedForProcessingDomainEvent`
- `PhotoProcessingStartedDomainEvent`
- `PhotoProcessingCompletedDomainEvent`
- `PhotoProcessingFailedDomainEvent`

## API Endpoints

### Photos

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/photos` | Upload one or more photos (multipart/form-data) |
| GET | `/api/photos` | List all photos (optional `?status=` filter) |
| GET | `/api/photos/{id}` | Get photo details by ID |

### Events

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/photos/{id}/events` | Get event log for a photo |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Health check endpoint |

## Configuration

### Connection String

Located in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=macbook-server;Port=5433;Database=RapidPhotoFlow;Username=postgres;Password=..."
  }
}
```

### Storage Path

```json
{
  "Storage": {
    "UploadPath": "./uploads"
  }
}
```

## Running the Application

### Prerequisites

- .NET 9 SDK
- PostgreSQL database

### Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run API
cd src/RapidPhotoFlow.Api
dotnet run
```

The API will be available at `http://localhost:7001`

### Database Migrations

Migrations are automatically applied on startup via `app.UseDatabaseMigration()`.

To manually manage migrations:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project src/RapidPhotoFlow.Infrastructure --startup-project src/RapidPhotoFlow.Api

# Apply migrations
dotnet ef database update --project src/RapidPhotoFlow.Infrastructure --startup-project src/RapidPhotoFlow.Api
```

## CORS Configuration

The API allows requests from `http://localhost:7002` (frontend).

## Background Processing

Photos are processed by a background worker (`PhotoProcessingWorker`) that:

1. Dequeues photos from an in-memory channel
2. Simulates processing with a 2-5 second delay
3. Updates photo status to `Processed` or `Failed` (10% failure rate)
4. Logs events to the event log

## Testing

Unit tests cover:

- Domain layer (Photo aggregate, PhotoId value object)
- Application layer (Query handlers)

Run tests with:

```bash
dotnet test
```

## Swagger UI

Access Swagger UI at `http://localhost:7001/swagger` when running in Development mode.

