
# RapidPhotoFlow Frontend Design (React + TypeScript)

This document defines the **RapidPhotoFlow frontend** architecture to sit on top of the RapidPhotoFlow backend service. It is intended to be copy‑pasted into Cursor and used as the blueprint for AI‑generated implementation.

---

## 1. Tech Stack & Rationale

### 1.1 Framework Choice: React over Vue

We will use **React 18 + TypeScript** instead of Vue because:

- The TeamFront RapidPhotoFlow challenge explicitly **recommends React** for the UI.
- We already have a **React starter architecture** defined in `AGENTS-React-UI` / Fusion Starter (React + Vite + Tailwind + React Router). Reusing this structure aligns with TeamFront standards and speeds up AI generation.
- React has first‑class support in Builder.io / Figma‑to‑code tools and matches our longer‑term micro‑frontend strategy.

Vue would also work technically, but **standardizing on React** gives us better reuse of existing infrastructure and documentation.

### 1.2 Frontend Stack

Based on `AGENTS-React-UI` / Fusion Starter, the stack will be:

- **Language**: TypeScript (strict mode)
- **Framework**: React 18
- **Routing**: React Router 6 (SPA mode)
- **Bundler/Dev server**: Vite
- **HTTP client**: Axios (wrapped via `apiService.ts`)
- **Data fetching**: TanStack React Query
- **Validation**: Zod schemas for API contracts
- **Styling**: TailwindCSS 3 + small custom components
- **Icons**: Lucide React (optional)
- **Package manager**: pnpm

This matches the patterns and directory structure outlined in the Fusion Starter doc and keeps the codebase familiar to other TeamFront projects.

---

## 2. User Flows & Screens

The challenge specifies three core screens: **Upload Photos**, **Processing Queue**, and **Review Photos**.

### 2.1 Global Layout

- **Top bar** with app name ("RapidPhotoFlow") and navigation tabs:
  - _Upload_
  - _Queue_
  - _Review_ (detail page is accessed via a list entry)
- **Content area** where the active route is rendered.
- Optional light/dark theme toggle.

We will implement this using a single `AppLayout` component (from `AGENTS-React-UI` patterns).

### 2.2 Screen: Upload Photos

**Route**: `/upload`

**Use cases**:

- User selects or drag‑drops **multiple photos**.
- For each photo:
  - Show file name, size, and local preview (if available).
  - On upload, show **optimistic status**: "Uploading / Queued".
- After successful upload:
  - Each item displays the **backend status** (`Uploaded` / `Queued` / `Processing` / `Processed` / `Failed`).
  - Option to **navigate to detail** for a single photo.

**Key UI elements**:

- `PhotoUploadDropzone` (drag‑drop + file picker)
- `PhotoUploadToolbar` (upload button, clear list)
- `PhotoUploadList` (per‑file rows with progress & status chips)

### 2.3 Screen: Processing Queue

**Route**: `/queue`

**Use cases**:

- Show a list / table of all photos currently in the system.
- Important fields:
  - Thumbnail (small preview)
  - File name
  - Status
  - Upload time
  - Processing start/finish times
- Ability to **filter** by status (e.g., All, Processing, Completed, Failed).
- Auto‑refresh / polling to keep statuses up to date.

**Key UI elements**:

- `PhotoQueueFilters` (status filter)
- `PhotoQueueTable` (rows with status badge + "View" button)
- `PhotoStatusBadge` (reusable component)

### 2.4 Screen: Review Photo

**Route**: `/photos/:id`

**Use cases**:

- Show a **large preview** of the photo.
- Show the **current status** + timestamps.
- Show a **workflow/event log** as a timeline (events like "Uploaded", "Queued", "Processing started", "Completed", "Failed").
- Provide navigation back to queue or upload.

**Key UI elements**:

- `PhotoDetailHeader` (file name, status, timestamps)
- `PhotoDetailImage` (image preview)
- `PhotoEventTimeline` (list of event log entries)

### 2.5 Optional: Home / Redirect

**Route**: `/`

- Auto‑redirect to `/upload`.

---

## 3. Backend API Contracts (Assumptions)

The frontend will talk to the backend service using the following endpoints (aligned with the backend design):

- `POST /api/photos`
  - Content type: `multipart/form-data`
  - Field: `files[]` (multiple files)
  - Response: `PhotoDto[]`
- `GET /api/photos`
  - Query: `status?`, `page?`, `pageSize?`
  - Response: `PhotoListItemDto[]`
- `GET /api/photos/:id`
  - Response: `PhotoDto`
- `GET /api/photos/:id/events`
  - Response: `PhotoEventDto[]`

### 3.1 TypeScript Models

We will define shared types in `src/lib/types/photos.ts`:

```ts
export type PhotoStatus = "Uploaded" | "Queued" | "Processing" | "Processed" | "Failed";

export interface PhotoDto {
  readonly id: string;
  readonly fileName: string;
  readonly contentType: string;
  readonly sizeBytes: number;
  readonly status: PhotoStatus;
  readonly uploadedAt: string; // ISO
  readonly processingStartedAt?: string;
  readonly processingCompletedAt?: string;
  readonly errorMessage?: string | null;
}

export interface PhotoListItemDto {
  readonly id: string;
  readonly fileName: string;
  readonly status: PhotoStatus;
  readonly uploadedAt: string;
}

export interface PhotoEventDto {
  readonly id: string;
  readonly photoId: string;
  readonly eventType: string;
  readonly message: string;
  readonly createdAt: string;
  readonly metadataJson?: unknown;
}
```

### 3.2 Zod Schemas

To validate API responses:

```ts
import { z } from "zod";

export const photoStatusSchema = z.enum([
  "Uploaded",
  "Queued",
  "Processing",
  "Processed",
  "Failed",
]);

export const photoDtoSchema = z.object({
  id: z.string().uuid(),
  fileName: z.string(),
  contentType: z.string(),
  sizeBytes: z.number().nonnegative(),
  status: photoStatusSchema,
  uploadedAt: z.string(),
  processingStartedAt: z.string().optional(),
  processingCompletedAt: z.string().optional(),
  errorMessage: z.string().nullable().optional(),
});

export const photoListItemDtoSchema = z.object({
  id: z.string().uuid(),
  fileName: z.string(),
  status: photoStatusSchema,
  uploadedAt: z.string(),
});

export const photoEventDtoSchema = z.object({
  id: z.string().uuid(),
  photoId: z.string().uuid(),
  eventType: z.string(),
  message: z.string(),
  createdAt: z.string(),
  metadataJson: z.unknown().optional(),
});

export const photoListSchema = z.array(photoListItemDtoSchema);
```

These schemas are used in API services and React Query loaders to ensure backend/FE contract safety.

---

## 4. Data Fetching & Client-Side State

### 4.1 React Query Setup

We will configure a `QueryClient` and provider in `src/app/providers/QueryProvider.tsx`:

- Global defaults:
  - `staleTime`: ~5–10 seconds for lists
  - `refetchOnWindowFocus`: `false`
- Error boundary integration for unexpected API errors.

### 4.2 Custom Hooks

We’ll create custom hooks under `src/features/*/hooks`:

- `useUploadPhotos()` – React Query mutation for `POST /api/photos`
- `usePhotosQuery(filters)` – query for `GET /api/photos`
- `usePhotoDetail(id)` – query for `GET /api/photos/:id`
- `usePhotoEvents(id)` – query for `GET /api/photos/:id/events`

Example hook signature:

```ts
export function usePhotosQuery(filters: { status?: PhotoStatus }) {
  return useQuery({
    queryKey: ["photos", filters],
    queryFn: () => photoApi.listPhotos(filters),
    refetchInterval: 5000, // auto-refresh queue
  });
}
```

### 4.3 State Management Strategy

- Prefer **React Query** for server state.
- Use **local component state** for ephemeral UI (selected files, dialogs).
- No global Redux/Zustand unless we later add cross‑cutting state.

---

## 5. Project Structure (Frontend)

We follow the Fusion Starter / feature‑based layout.

```text
frontend/
  package.json
  pnpm-lock.yaml
  index.html
  vite.config.ts
  tsconfig.json
  tailwind.config.ts
  postcss.config.cjs

  src/
    app/
      App.tsx
      main.tsx
      layout/
        AppLayout.tsx
        Sidebar.tsx     # Optional, could be minimal
        Topbar.tsx
      routes/
        index.tsx
      providers/
        QueryProvider.tsx
        ThemeProvider.tsx
      config/
        env.ts
      styles/
        globals.css

    lib/
      api/
        apiClient.ts     # axios instance
        photoApi.ts      # photos-specific endpoints
      hooks/
        useDebounce.ts
      ui/
        Button.tsx
        Input.tsx
        Badge.tsx
        Card.tsx
        Spinner.tsx
        Tabs.tsx
      utils/
        formatDate.ts
        formatFileSize.ts
      constants/
        routes.ts
      types/
        photos.ts        # shared PhotoDto, schemas

    features/
      photo-upload/
        components/
          PhotoUploadDropzone.tsx
          PhotoUploadList.tsx
          PhotoUploadItem.tsx
        hooks/
          usePhotoUpload.ts
        api/
          index.ts        # re-export from lib/api/photoApi if needed
        models/
          uploadModels.ts
        PhotoUpload.tsx   # routed container

      photo-queue/
        components/
          PhotoQueueFilters.tsx
          PhotoQueueTable.tsx
          PhotoQueueRow.tsx
        hooks/
          usePhotoQueue.ts
        models/
          queueFilters.ts
        PhotoQueue.tsx    # routed container

      photo-review/
        components/
          PhotoDetailHeader.tsx
          PhotoDetailImage.tsx
          PhotoEventTimeline.tsx
        hooks/
          usePhotoDetail.ts
          usePhotoEvents.ts
        PhotoReview.tsx   # routed container

    assets/
      images/
      icons/
```

This structure aligns with `AGENTS-React-UI`: containers in `features`, shared UI in `lib/ui`, shared helpers in `lib/utils`, etc.

---

## 6. Routing Design

Define routes in `src/app/routes/index.tsx`:

- `/` → redirects to `/upload`
- `/upload` → `<PhotoUpload />`
- `/queue` → `<PhotoQueue />`
- `/photos/:id` → `<PhotoReview />`

Sketch:

```tsx
import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppLayout } from "../layout/AppLayout";
import { PhotoUpload } from "../../features/photo-upload/PhotoUpload";
import { PhotoQueue } from "../../features/photo-queue/PhotoQueue";
import { PhotoReview } from "../../features/photo-review/PhotoReview";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/upload" replace /> },
      { path: "upload", element: <PhotoUpload /> },
      { path: "queue", element: <PhotoQueue /> },
      { path: "photos/:id", element: <PhotoReview /> },
    ],
  },
]);
```

`AppLayout` will render the topbar and navigation tabs for Upload / Queue.

---

## 7. Feature Design & Component Sketches

### 7.1 photo-upload Feature

**Container**: `PhotoUpload.tsx`

Responsibilities:

- Manage local list of selected files (before upload).
- Call `useUploadPhotos()` to send files to backend.
- Show upload progress & success/failure states.

Pseudo‑implementation:

```tsx
export function PhotoUpload() {
  const [files, setFiles] = useState<File[]>([]);
  const uploadMutation = useUploadPhotos();

  const handleFilesSelected = (newFiles: FileList | File[]) => {
    setFiles(prev => [...prev, ...Array.from(newFiles)]);
  };

  const handleUpload = () => {
    if (!files.length) return;
    uploadMutation.mutate(files);
  };

  return (
    <div className="space-y-4">
      <PhotoUploadDropzone onFilesSelected={handleFilesSelected} />
      <div className="flex justify-between items-center">
        <p className="text-sm text-muted-foreground">
          {files.length} file(s) selected.
        </p>
        <Button onClick={handleUpload} disabled={!files.length || uploadMutation.isLoading}>
          Upload
        </Button>
      </div>
      <PhotoUploadList
        files={files}
        statusByFile={uploadMutation.data}
        isLoading={uploadMutation.isLoading}
      />
    </div>
  );
}
```

### 7.2 photo-queue Feature

**Container**: `PhotoQueue.tsx`

Responsibilities:

- Read filters (status) from query string or local state.
- Use `usePhotoQueue()` (wraps `usePhotosQuery`) for data.
- Render table with status badges and "View" action linking to `/photos/:id`.

Key behaviors:

- Automatic polling every ~5 seconds while tab is active.
- Allow manual "Refresh" button.

### 7.3 photo-review Feature

**Container**: `PhotoReview.tsx`

Responsibilities:

- Read `:id` from router params.
- Fetch `PhotoDto` via `usePhotoDetail(id)`.
- Fetch events via `usePhotoEvents(id)`.
- Render image + statuses + event timeline.

---

## 8. Upload & Concurrency Behavior

### 8.1 Multiple Photo Uploads

Upload flow:

1. User selects multiple files.
2. Frontend builds `FormData` with `files[]` entries.
3. Axios request to `POST /api/photos`.
4. On success:
   - Invalidate `["photos"]` query so queue view refreshes.
   - Optionally show a toast with upload summary.

Axios wrapper (`photoApi.uploadPhotos`) should support **parallel uploads** if needed (e.g., chunking large file sets) but a single multipart request per batch is sufficient for this challenge.

### 8.2 Status Updates

Base version: **polling** via React Query:

- `usePhotosQuery` uses `refetchInterval` (e.g., 5000 ms) on `/queue` route.
- `usePhotoDetail` can have a smaller interval (e.g., 2000 ms) while photo is in non‑terminal status (`Processing` / `Queued`).

Optional enhancement: SignalR/WebSocket subscription:

- If backend exposes a SignalR hub, we add `usePhotoRealtimeUpdates(photoId)` to subscribe and patch React Query cache with pushed updates.
- Implementation can be feature‑flagged so polling remains the baseline.

---

## 9. Styling & UX Details

- **TailwindCSS** for layout and utility classes.
- Shared UI components live in `src/lib/ui`:
  - `Button`, `Input`, `Badge`, `Card`, `Spinner`, `Tabs`.
- Status badges use consistent colors:
  - `Queued` → gray
  - `Processing` → blue
  - `Processed` → green
  - `Failed` → red

Typography and spacing follow a minimal modern design:

- Large headings for page titles.
- Cards for grouping content (e.g., photo detail + events).
- Hover states on interactive rows and buttons.

Accessibility:

- Proper `aria-label`s on upload dropzone and navigation.
- Keyboard focus styles.
- Use semantic HTML for tables and timelines.

---

## 10. Implementation Plan (for Cursor)

1. **Scaffold project** with Vite + React + TS + Tailwind (or start from Fusion Starter template).
2. **Add dependencies**:
   - `pnpm add axios @tanstack/react-query zod @tanstack/react-query-devtools`
3. **Set up app shell**:
   - `App.tsx`, `AppLayout`, routes (Upload / Queue / Photo detail).
4. **Implement API layer**:
   - `apiClient.ts` (axios instance with base URL from env).
   - `photoApi.ts` with methods: `uploadPhotos`, `listPhotos`, `getPhoto`, `getPhotoEvents`.
5. **Define models & schemas** in `lib/types/photos.ts`.
6. **Implement features** in order:
   - `photo-upload`
   - `photo-queue`
   - `photo-review`
7. **Wire React Query** & add polling/refetch behavior.
8. **Polish UI** with cards, badges, and responsive layout.
9. **(Optional)**: add real‑time updates via SignalR hook if backend supports it.

This design is intentionally **implementation‑ready**: Cursor can generate each folder, file, and component, using this as the high‑level blueprint.
