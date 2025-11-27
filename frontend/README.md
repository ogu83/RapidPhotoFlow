# RapidPhotoFlow Frontend Documentation

## Overview

RapidPhotoFlow frontend is a modern React 18 application built with TypeScript, providing a sleek user interface for photo upload, processing queue management, and photo review workflows.

## Architecture

### Project Structure

```
frontend/
├── src/
│   ├── app/                    # Application shell
│   │   ├── layout/             # Global layout components
│   │   ├── providers/          # React Query provider
│   │   └── routes/             # React Router configuration
│   ├── features/               # Feature modules
│   │   ├── photo-upload/       # Upload functionality
│   │   ├── photo-queue/        # Queue management
│   │   └── photo-review/       # Photo detail & preview
│   ├── lib/                    # Shared libraries
│   │   ├── api/                # API client & endpoints
│   │   ├── types/              # TypeScript types & Zod schemas
│   │   ├── ui/                 # Reusable UI components
│   │   └── utils/              # Utility functions
│   ├── App.tsx                 # Root component
│   └── main.tsx                # Entry point
├── public/                     # Static assets
├── index.html                  # HTML template
└── package.json                # Dependencies
```

## Key Technologies

- **React 18** - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool & dev server
- **TailwindCSS** - Utility-first CSS
- **React Router 6** - Client-side routing
- **TanStack React Query** - Data fetching & caching
- **Axios** - HTTP client
- **Zod** - Schema validation
- **Lucide React** - Icons

## Features

### Photo Upload (`/upload`)

- Drag-and-drop file selection
- Multi-file upload support
- Upload progress indication
- Automatic navigation to queue after upload

### Photo Queue (`/queue`)

- Real-time status updates (5-second polling)
- Status filter tabs: All, Queued, Processing, Processed, Failed
- Quick actions: View details, Delete photo
- Relative timestamp display

### Photo Review (`/photos/:id`)

- Full photo details with metadata
- Image preview for processed photos
- Fullscreen image viewer with zoom
- Event timeline showing processing history
- Delete functionality with confirmation

## UI Components

### Shared Components (`src/lib/ui/`)

| Component | Description |
|-----------|-------------|
| `Button` | Styled button with variants |
| `Badge` | Status indicator badges |
| `Card` | Container component |
| `Spinner` | Loading indicator |
| `PhotoStatusBadge` | Color-coded status display |

### Status Color Coding

| Status | Color |
|--------|-------|
| Queued | Yellow |
| Processing | Blue (animated) |
| Processed | Green |
| Failed | Red |

## API Integration

### API Client (`src/lib/api/apiClient.ts`)

Axios instance configured with:
- Base URL from environment variable
- Request/response logging
- Error handling interceptors

### API Endpoints (`src/lib/api/photoApi.ts`)

| Method | Description |
|--------|-------------|
| `uploadPhotos(files)` | Upload multiple photos |
| `listPhotos(params?)` | List photos with optional filter |
| `getPhoto(id)` | Get photo details |
| `getPhotoEvents(id)` | Get event history |
| `deletePhoto(id)` | Delete a photo |

## Routing

| Path | Component | Description |
|------|-----------|-------------|
| `/` | Redirect | Redirects to `/upload` |
| `/upload` | `PhotoUpload` | Upload interface |
| `/queue` | `PhotoQueue` | Processing queue |
| `/photos/:id` | `PhotoReview` | Photo details |

## Configuration

### Environment Variables

Create a `.env` file:

```env
VITE_API_BASE_URL=http://localhost:7001
```

For Docker deployment, this is set to empty string (nginx proxies API calls).

### Development Server

```bash
# Install dependencies
npm install

# Start dev server (port 7002)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Data Fetching

### React Query Hooks

| Hook | Purpose |
|------|---------|
| `usePhotoUpload` | Upload mutation |
| `usePhotoQueue` | List photos with polling |
| `usePhotoDetail` | Single photo query |
| `usePhotoEvents` | Event log query |
| `useDeletePhoto` | Delete mutation |

### Polling Configuration

The queue page polls for updates every 5 seconds:

```typescript
refetchInterval: 5000
```

## Styling

### TailwindCSS Configuration

Custom color palette with primary colors and dark theme optimized for the application.

### Design System

- Dark theme with slate color palette
- Primary accent color: Blue/Cyan gradient
- Rounded corners and subtle shadows
- Smooth transitions and animations

## Build & Deployment

### Production Build

```bash
npm run build
```

Output is generated in `dist/` folder.

### Docker Deployment

The frontend is containerized with nginx:
- Multi-stage build for optimization
- Nginx serves static files
- API requests proxied to backend
- SPA routing handled properly

See root `DOCKER.md` for deployment instructions.

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

