# Docker Deployment Guide

## Quick Start

Run the entire application stack with a single command:

```bash
docker compose up --build
```

This will start:
- **PostgreSQL** database (internal, not exposed)
- **Backend API** (.NET 9.0, internal, not exposed)
- **Frontend UI** (React + Nginx, exposed on `http://localhost:7002`)

## Access the Application

Open your browser and navigate to:
```
http://localhost:7002
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Network                            │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐      │
│  │  PostgreSQL │◄───│  .NET API   │◄───│   Nginx     │◄─────┼──► localhost:7002
│  │    (db)     │    │   (api)     │    │    (ui)     │      │
│  │  Port 5432  │    │  Port 7001  │    │   Port 80   │      │
│  └─────────────┘    └─────────────┘    └─────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

- **Only the UI (port 7002)** is exposed to the host machine
- API calls from the frontend are proxied through Nginx to the backend
- Database is only accessible within the Docker network

## Commands

### Start the stack
```bash
docker compose up -d
```

### Start with rebuild
```bash
docker compose up --build -d
```

### View logs
```bash
docker compose logs -f
```

### View specific service logs
```bash
docker compose logs -f api
docker compose logs -f ui
docker compose logs -f db
```

### Stop the stack
```bash
docker compose down
```

### Stop and remove volumes (clean reset)
```bash
docker compose down -v
```

## Data Persistence

The following data is persisted in Docker volumes:
- `postgres_data` - Database files
- `uploads_data` - Uploaded photo files
- `logs_data` - Application logs

To view volumes:
```bash
docker volume ls | grep rapidphotoflow
```

## Environment Variables

The following environment variables can be customized in `docker-compose.yml`:

### Database
- `POSTGRES_DB` - Database name (default: `RapidPhotoFlow`)
- `POSTGRES_USER` - Database user (default: `postgres`)
- `POSTGRES_PASSWORD` - Database password

### API
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `Storage__UploadPath` - Path for uploaded files
- `Cors__AllowedOrigins__0` - Allowed CORS origin

## Troubleshooting

### Database connection issues
```bash
# Check if database is healthy
docker compose ps

# Check database logs
docker compose logs db
```

### API not starting
```bash
# Check API logs
docker compose logs api

# Verify database is ready
docker compose exec db pg_isready -U postgres
```

### Rebuild a specific service
```bash
docker compose build api
docker compose up -d api
```

