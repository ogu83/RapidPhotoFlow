import { useState } from "react";
import { Link } from "react-router-dom";
import { Eye, Trash2, Loader2 } from "lucide-react";
import { PhotoListItemDto } from "../../../lib/types/photos";
import { PhotoStatusBadge } from "../../../lib/ui/PhotoStatusBadge";
import { formatRelativeTime } from "../../../lib/utils/formatDate";
import { useDeletePhoto } from "../hooks/useDeletePhoto";

interface PhotoQueueTableProps {
  photos: PhotoListItemDto[];
}

export function PhotoQueueTable({ photos }: PhotoQueueTableProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);
  const deletePhoto = useDeletePhoto();

  const handleDelete = async (photoId: string) => {
    setDeletingId(photoId);
    try {
      await deletePhoto.mutateAsync(photoId);
    } finally {
      setDeletingId(null);
      setConfirmDeleteId(null);
    }
  };

  if (photos.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-slate-400">No photos found</p>
      </div>
    );
  }

  return (
    <>
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-slate-700">
              <th className="text-left py-3 px-4 text-sm font-medium text-slate-400">
                File Name
              </th>
              <th className="text-left py-3 px-4 text-sm font-medium text-slate-400">
                Status
              </th>
              <th className="text-left py-3 px-4 text-sm font-medium text-slate-400">
                Uploaded
              </th>
              <th className="text-right py-3 px-4 text-sm font-medium text-slate-400">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {photos.map((photo) => (
              <tr
                key={photo.id}
                className="border-b border-slate-700/50 hover:bg-slate-800/50 transition-colors"
              >
                <td className="py-3 px-4">
                  <span className="text-sm font-medium text-slate-200 truncate max-w-xs block">
                    {photo.fileName}
                  </span>
                </td>
                <td className="py-3 px-4">
                  <PhotoStatusBadge status={photo.status} />
                </td>
                <td className="py-3 px-4">
                  <span className="text-sm text-slate-400">
                    {formatRelativeTime(photo.uploadedAt)}
                  </span>
                </td>
                <td className="py-3 px-4 text-right">
                  <div className="flex items-center justify-end gap-2">
                    <Link
                      to={`/photos/${photo.id}`}
                      className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-primary-400 hover:text-primary-300 hover:bg-primary-500/10 rounded-lg transition-colors"
                    >
                      <Eye className="w-4 h-4" />
                      View
                    </Link>
                    <button
                      onClick={() => setConfirmDeleteId(photo.id)}
                      disabled={deletingId === photo.id}
                      className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-red-400 hover:text-red-300 hover:bg-red-500/10 rounded-lg transition-colors disabled:opacity-50"
                    >
                      {deletingId === photo.id ? (
                        <Loader2 className="w-4 h-4 animate-spin" />
                      ) : (
                        <Trash2 className="w-4 h-4" />
                      )}
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Delete Confirmation Modal */}
      {confirmDeleteId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70">
          <div className="bg-slate-800 border border-slate-700 rounded-xl p-6 max-w-md w-full mx-4 shadow-xl">
            <h3 className="text-lg font-semibold text-slate-100 mb-2">
              Delete Photo?
            </h3>
            <p className="text-slate-400 mb-6">
              Are you sure you want to delete this photo? This action cannot be undone.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setConfirmDeleteId(null)}
                className="px-4 py-2 text-sm font-medium text-slate-300 bg-slate-700 hover:bg-slate-600 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDelete(confirmDeleteId)}
                disabled={deletingId === confirmDeleteId}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-500 rounded-lg transition-colors disabled:opacity-50 inline-flex items-center gap-2"
              >
                {deletingId === confirmDeleteId && (
                  <Loader2 className="w-4 h-4 animate-spin" />
                )}
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}
