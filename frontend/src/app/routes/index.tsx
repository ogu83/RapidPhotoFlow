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

