import { useMutation, useQueryClient } from "@tanstack/react-query";
import { photoApi } from "../../../lib/api/photoApi";

export function usePhotoUpload() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (files: File[]) => photoApi.uploadPhotos(files),
    onSuccess: () => {
      // Invalidate the photos list query to refresh the queue
      queryClient.invalidateQueries({ queryKey: ["photos"] });
    },
  });
}

