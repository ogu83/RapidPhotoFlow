# RapidPhotoFlow

A lightweight photo upload, processing, and review workflow application built as part of a full-stack development challenge.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)

## 🎯 Project Overview

### Project Video
https://teamfront-my.sharepoint.com/:v:/g/personal/oguz_arborgold_com/EWHGdWLLAMlCl15hgFz785UBzEphIcm6j6Jy4F275k4gcw?e=ThUASB

RapidPhotoFlow demonstrates a complete full-stack application with:

- **Photo Upload**: Drag-and-drop multi-file upload interface
- **Processing Queue**: Real-time status tracking with automatic polling
- **Photo Review**: Detailed view with image preview and event timeline
- **Background Processing**: Simulated async photo processing workflow

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         RapidPhotoFlow                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   ┌──────────────────┐         ┌──────────────────┐                 │
│   │     Frontend     │  HTTP   │     Backend      │                 │
│   │   React + Vite   │────────▶│   .NET 9 API     │                 │
│   │   Port: 7002     │         │   Port: 7001     │                 │
│   └──────────────────┘         └────────┬─────────┘                 │
│                                         │                            │
│                                         ▼                            │
│                                ┌──────────────────┐                 │
│                                │   PostgreSQL     │                 │
│                                │   Port: 5432     │                 │
│                                └──────────────────┘                 │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** with Clean Architecture
- **CQRS Pattern** using MediatR
- **Entity Framework Core 9** with PostgreSQL
- **Serilog** for structured logging
- **Background Services** for async processing

### Frontend
- **React 18** with TypeScript
- **Vite** for fast development
- **TailwindCSS** for styling
- **TanStack React Query** for data fetching
- **React Router 6** for navigation

### Infrastructure
- **Docker & Docker Compose** for containerization
- **Nginx** for frontend serving and API proxying
- **PostgreSQL 16** for data persistence

## 🚀 Quick Start with Docker

The easiest way to run the application is with Docker Compose:

```bash
# Clone the repository
git clone <repository-url>
cd RapidPhotoFlow

# Start everything
docker compose up --build

# Or run in background
docker compose up --build -d
```

Access the application at: **http://localhost:7002**

### Docker Commands

| Command | Description |
|---------|-------------|
| `docker compose up --build` | Build and start all services |
| `docker compose up --build -d` | Build and start in background |
| `docker compose logs -f` | View logs from all services |
| `docker compose down` | Stop all services |
| `docker compose down -v` | Stop and remove all data |

## 💻 Local Development

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/)

### Backend

```bash
cd backend

# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Start API (http://localhost:7001)
cd src/RapidPhotoFlow.Api
dotnet run
```

### Frontend

```bash
cd frontend

# Install dependencies
npm install

# Start dev server (http://localhost:7002)
npm run dev
```

## 📁 Project Structure

```
RapidPhotoFlow/
├── backend/                    # .NET Backend
│   ├── src/
│   │   ├── RapidPhotoFlow.Domain/         # Domain entities & events
│   │   ├── RapidPhotoFlow.Application/    # CQRS commands & queries
│   │   ├── RapidPhotoFlow.Infrastructure/ # EF Core & services
│   │   └── RapidPhotoFlow.Api/            # Minimal API endpoints
│   ├── tests/
│   │   └── RapidPhotoFlow.UnitTests/      # Unit tests
│   ├── Docs/
│   │   └── README.md                       # Backend documentation
│   └── Dockerfile
├── frontend/                   # React Frontend
│   ├── src/
│   │   ├── app/                # App shell & routing
│   │   ├── features/           # Feature modules
│   │   └── lib/                # Shared components & utils
│   ├── README.md               # Frontend documentation
│   └── Dockerfile
├── docker-compose.yml          # Docker orchestration
├── DOCKER.md                   # Docker deployment guide
└── README.md                   # This file
```

## 📋 Features

### Photo Upload
- Drag-and-drop interface
- Multiple file selection
- Supported formats: JPEG, PNG, GIF, WebP

### Processing Queue
- Real-time status updates (5-second polling)
- Filter by status: All, Queued, Processing, Processed, Failed
- Delete photos with confirmation

### Photo Review
- Full photo metadata display
- Image preview with fullscreen mode
- Event timeline showing processing history
- Delete functionality

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/photos` | Upload photos |
| `GET` | `/api/photos` | List photos |
| `GET` | `/api/photos/{id}` | Get photo details |
| `GET` | `/api/photos/{id}/file` | Get photo file |
| `GET` | `/api/photos/{id}/events` | Get event log |
| `DELETE` | `/api/photos/{id}` | Delete photo |

## 🧪 Testing

### Run Backend Tests

```bash
cd backend
dotnet test --verbosity normal
```

**Test Coverage:**
- Domain layer (Photo aggregate, PhotoId value object)
- Application layer (Query & Command handlers)
- 30 unit tests total

## 📖 Documentation

- **[Backend Documentation](backend/Docs/README.md)** - API details, architecture, configuration
- **[Frontend Documentation](frontend/README.md)** - Components, routing, data fetching
- **[Docker Guide](DOCKER.md)** - Containerization and deployment

## 🔧 Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=RapidPhotoFlow;..."
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:7002"]
  }
}
```

### Frontend (`.env`)

```env
VITE_API_BASE_URL=http://localhost:7001
```

## 🏛️ Design Patterns

- **Clean Architecture** - Separation of concerns with Domain, Application, Infrastructure layers
- **CQRS** - Command Query Responsibility Segregation using MediatR
- **Repository Pattern** - Abstracted data access
- **Unit of Work** - Transaction management
- **Domain Events** - Event-driven state transitions

## 📜 License

This project was developed as part of a full-stack development challenge.

---

Built with ❤️ using .NET 9, React 18, and PostgreSQL

