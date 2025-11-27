import { apiClient } from "./apiClient";
import {
  PhotoDto,
  PhotoListItemDto,
  PhotoEventDto,
  PhotoStatus,
  photoDtoArraySchema,
  photoListSchema,
  photoDtoSchema,
  photoEventArraySchema,
} from "../types/photos";

export interface ListPhotosParams {
  status?: PhotoStatus;
}

export const photoApi = {
  /**
   * Upload one or more photos.
   */
  async uploadPhotos(files: File[]): Promise<PhotoDto[]> {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append("files", file);
    });

    const response = await apiClient.post<PhotoDto[]>("/api/photos", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return photoDtoArraySchema.parse(response.data);
  },

  /**
   * List all photos with optional status filter.
   */
  async listPhotos(params?: ListPhotosParams): Promise<PhotoListItemDto[]> {
    const queryParams = new URLSearchParams();
    if (params?.status) {
      queryParams.set("status", params.status);
    }

    const url = queryParams.toString()
      ? `/api/photos?${queryParams.toString()}`
      : "/api/photos";

    const response = await apiClient.get<PhotoListItemDto[]>(url);
    return photoListSchema.parse(response.data);
  },

  /**
   * Get photo details by ID.
   */
  async getPhoto(id: string): Promise<PhotoDto | null> {
    try {
      const response = await apiClient.get<PhotoDto>(`/api/photos/${id}`);
      return photoDtoSchema.parse(response.data);
    } catch (error: unknown) {
      if (error && typeof error === "object" && "response" in error) {
        const axiosError = error as { response?: { status?: number } };
        if (axiosError.response?.status === 404) {
          return null;
        }
      }
      throw error;
    }
  },

  /**
   * Get event log for a photo.
   */
  async getPhotoEvents(photoId: string): Promise<PhotoEventDto[]> {
    const response = await apiClient.get<PhotoEventDto[]>(
      `/api/photos/${photoId}/events`
    );
    return photoEventArraySchema.parse(response.data);
  },
};

