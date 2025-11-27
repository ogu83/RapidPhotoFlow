import { z } from "zod";

// Photo status type and schema
export type PhotoStatus = "Uploaded" | "Queued" | "Processing" | "Processed" | "Failed";

export const photoStatusSchema = z.enum([
  "Uploaded",
  "Queued",
  "Processing",
  "Processed",
  "Failed",
]);

// Full photo DTO
export interface PhotoDto {
  readonly id: string;
  readonly fileName: string;
  readonly contentType: string;
  readonly sizeBytes: number;
  readonly status: PhotoStatus;
  readonly storagePath: string;
  readonly uploadedAt: string;
  readonly processingStartedAt?: string | null;
  readonly processingCompletedAt?: string | null;
  readonly errorMessage?: string | null;
}

export const photoDtoSchema = z.object({
  id: z.string().uuid(),
  fileName: z.string(),
  contentType: z.string(),
  sizeBytes: z.number().nonnegative(),
  status: photoStatusSchema,
  storagePath: z.string(),
  uploadedAt: z.string(),
  processingStartedAt: z.string().nullable().optional(),
  processingCompletedAt: z.string().nullable().optional(),
  errorMessage: z.string().nullable().optional(),
});

// Photo list item DTO
export interface PhotoListItemDto {
  readonly id: string;
  readonly fileName: string;
  readonly status: PhotoStatus;
  readonly uploadedAt: string;
}

export const photoListItemDtoSchema = z.object({
  id: z.string().uuid(),
  fileName: z.string(),
  status: photoStatusSchema,
  uploadedAt: z.string(),
});

// Photo event DTO
export interface PhotoEventDto {
  readonly id: string;
  readonly photoId: string;
  readonly eventType: string;
  readonly message: string;
  readonly createdAt: string;
  readonly metadataJson?: string | null;
}

export const photoEventDtoSchema = z.object({
  id: z.string().uuid(),
  photoId: z.string().uuid(),
  eventType: z.string(),
  message: z.string(),
  createdAt: z.string(),
  metadataJson: z.string().nullable().optional(),
});

// Array schemas
export const photoListSchema = z.array(photoListItemDtoSchema);
export const photoDtoArraySchema = z.array(photoDtoSchema);
export const photoEventArraySchema = z.array(photoEventDtoSchema);

