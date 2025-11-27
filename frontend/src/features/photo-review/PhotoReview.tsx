import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { ArrowLeft, History, ImageIcon, Trash2, Loader2 } from "lucide-react";
import { Card, CardContent, CardHeader } from "../../lib/ui/Card";
import { Spinner } from "../../lib/ui/Spinner";
import { PhotoDetailHeader } from "./components/PhotoDetailHeader";
import { PhotoEventTimeline } from "./components/PhotoEventTimeline";
import { PhotoPreview } from "./components/PhotoPreview";
import { usePhotoDetail } from "./hooks/usePhotoDetail";
import { usePhotoEvents } from "./hooks/usePhotoEvents";
import { useDeletePhoto } from "../photo-queue/hooks/useDeletePhoto";

export function PhotoReview() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const { data: photo, isLoading: isLoadingPhoto } = usePhotoDetail(id!);
  const { data: events, isLoading: isLoadingEvents } = usePhotoEvents(id!);
  const deletePhoto = useDeletePhoto();
  
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const handleDelete = async () => {
    if (!id) return;
    
    try {
      await deletePhoto.mutateAsync(id);
      navigate("/queue");
    } catch (error) {
      console.error("Failed to delete photo:", error);
    }
  };

  if (isLoadingPhoto) {
    return (
      <div className="flex justify-center py-24">
        <Spinner size="lg" />
      </div>
    );
  }

  if (!photo) {
    return (
      <div className="max-w-2xl mx-auto text-center py-24">
        <h2 className="text-xl font-semibold text-slate-200">Photo Not Found</h2>
        <p className="text-slate-400 mt-2">
          The photo you're looking for doesn't exist.
        </p>
        <Link
          to="/queue"
          className="inline-flex items-center gap-2 mt-6 text-primary-400 hover:text-primary-300"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Queue
        </Link>
      </div>
    );
  }

  return (
    <>
      <div className="space-y-6">
        {/* Back navigation and actions */}
        <div className="flex items-center justify-between">
          <Link
            to="/queue"
            className="inline-flex items-center gap-2 text-slate-400 hover:text-slate-200 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Queue
          </Link>
          
          <button
            onClick={() => setShowDeleteConfirm(true)}
            className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-red-400 hover:text-red-300 bg-red-500/10 hover:bg-red-500/20 border border-red-500/30 rounded-lg transition-colors"
          >
            <Trash2 className="w-4 h-4" />
            Delete Photo
          </button>
        </div>

        {/* Photo Details */}
        <Card>
          <CardContent className="pt-6">
            <PhotoDetailHeader photo={photo} />
          </CardContent>
        </Card>

        {/* Photo Preview */}
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <ImageIcon className="w-5 h-5 text-primary-400" />
              <h2 className="text-lg font-semibold text-slate-100">
                Photo Preview
              </h2>
            </div>
          </CardHeader>
          <CardContent>
            <PhotoPreview photo={photo} />
          </CardContent>
        </Card>

        {/* Event Timeline */}
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <History className="w-5 h-5 text-primary-400" />
              <h2 className="text-lg font-semibold text-slate-100">
                Event Timeline
              </h2>
              {events && (
                <span className="text-sm text-slate-500">
                  ({events.length} event{events.length !== 1 ? "s" : ""})
                </span>
              )}
            </div>
          </CardHeader>
          <CardContent>
            {isLoadingEvents ? (
              <div className="flex justify-center py-8">
                <Spinner />
              </div>
            ) : (
              <PhotoEventTimeline events={events || []} />
            )}
          </CardContent>
        </Card>
      </div>

      {/* Delete Confirmation Modal */}
      {showDeleteConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70">
          <div className="bg-slate-800 border border-slate-700 rounded-xl p-6 max-w-md w-full mx-4 shadow-xl">
            <h3 className="text-lg font-semibold text-slate-100 mb-2">
              Delete Photo?
            </h3>
            <p className="text-slate-400 mb-6">
              Are you sure you want to delete "{photo.fileName}"? This action cannot be undone.
            </p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowDeleteConfirm(false)}
                className="px-4 py-2 text-sm font-medium text-slate-300 bg-slate-700 hover:bg-slate-600 rounded-lg transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleDelete}
                disabled={deletePhoto.isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-500 rounded-lg transition-colors disabled:opacity-50 inline-flex items-center gap-2"
              >
                {deletePhoto.isPending && (
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
