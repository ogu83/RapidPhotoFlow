import { useQuery } from "@tanstack/react-query";
import { photoApi } from "../../../lib/api/photoApi";

export function usePhotoDetail(id: string) {
  return useQuery({
    queryKey: ["photo", id],
    queryFn: () => photoApi.getPhoto(id),
    enabled: !!id,
    refetchInterval: (query) => {
      // Poll more frequently if photo is not in terminal state
      const photo = query.state.data;
      if (!photo) return false;
      if (photo.status === "Processing" || photo.status === "Queued") {
        return 2000; // 2 seconds
      }
      return false; // Stop polling
    },
  });
}

