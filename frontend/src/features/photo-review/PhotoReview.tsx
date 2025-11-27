import { useParams, Link } from "react-router-dom";
import { ArrowLeft, History } from "lucide-react";
import { Card, CardContent, CardHeader } from "../../lib/ui/Card";
import { Spinner } from "../../lib/ui/Spinner";
import { PhotoDetailHeader } from "./components/PhotoDetailHeader";
import { PhotoEventTimeline } from "./components/PhotoEventTimeline";
import { usePhotoDetail } from "./hooks/usePhotoDetail";
import { usePhotoEvents } from "./hooks/usePhotoEvents";

export function PhotoReview() {
  const { id } = useParams<{ id: string }>();
  
  const { data: photo, isLoading: isLoadingPhoto } = usePhotoDetail(id!);
  const { data: events, isLoading: isLoadingEvents } = usePhotoEvents(id!);

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
    <div className="space-y-6">
      {/* Back navigation */}
      <Link
        to="/queue"
        className="inline-flex items-center gap-2 text-slate-400 hover:text-slate-200 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        Back to Queue
      </Link>

      {/* Photo Details */}
      <Card>
        <CardContent className="pt-6">
          <PhotoDetailHeader photo={photo} />
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
  );
}

