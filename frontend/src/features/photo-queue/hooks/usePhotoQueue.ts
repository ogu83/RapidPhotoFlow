import { useQuery } from "@tanstack/react-query";
import { photoApi, ListPhotosParams } from "../../../lib/api/photoApi";

export function usePhotoQueue(params?: ListPhotosParams) {
  return useQuery({
    queryKey: ["photos", params],
    queryFn: () => photoApi.listPhotos(params),
    refetchInterval: 5000, // Refresh every 5 seconds
  });
}

