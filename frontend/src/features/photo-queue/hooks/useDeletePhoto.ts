import { useMutation, useQueryClient } from "@tanstack/react-query";
import { photoApi } from "../../../lib/api/photoApi";

export function useDeletePhoto() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (photoId: string) => photoApi.deletePhoto(photoId),
    onSuccess: () => {
      // Invalidate the photos list to refresh the data
      queryClient.invalidateQueries({ queryKey: ["photos"] });
    },
  });
}

