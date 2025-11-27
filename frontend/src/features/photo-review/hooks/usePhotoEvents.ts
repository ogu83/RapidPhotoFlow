import { useQuery } from "@tanstack/react-query";
import { photoApi } from "../../../lib/api/photoApi";

export function usePhotoEvents(photoId: string) {
  return useQuery({
    queryKey: ["photo-events", photoId],
    queryFn: () => photoApi.getPhotoEvents(photoId),
    enabled: !!photoId,
    refetchInterval: 5000, // Poll every 5 seconds
  });
}

