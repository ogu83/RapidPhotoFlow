import { useState } from "react";
import { RefreshCw, ListTodo } from "lucide-react";
import { Card, CardContent, CardHeader } from "../../lib/ui/Card";
import { Button } from "../../lib/ui/Button";
import { Spinner } from "../../lib/ui/Spinner";
import { PhotoQueueFilters } from "./components/PhotoQueueFilters";
import { PhotoQueueTable } from "./components/PhotoQueueTable";
import { usePhotoQueue } from "./hooks/usePhotoQueue";
import { PhotoStatus } from "../../lib/types/photos";

export function PhotoQueue() {
  const [statusFilter, setStatusFilter] = useState<PhotoStatus | "all">("all");
  
  const { data: photos, isLoading, refetch, isFetching } = usePhotoQueue(
    statusFilter === "all" ? undefined : { status: statusFilter }
  );

  const handleRefresh = () => {
    refetch();
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-100">Processing Queue</h1>
          <p className="text-slate-400 mt-1">
            Track the status of your uploaded photos
          </p>
        </div>
        <Button variant="secondary" onClick={handleRefresh} disabled={isFetching}>
          <RefreshCw className={`w-4 h-4 mr-2 ${isFetching ? "animate-spin" : ""}`} />
          Refresh
        </Button>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <ListTodo className="w-5 h-5 text-primary-400" />
              <h2 className="text-lg font-semibold text-slate-100">
                Photo Queue
              </h2>
              {photos && (
                <span className="text-sm text-slate-500">
                  ({photos.length} photo{photos.length !== 1 ? "s" : ""})
                </span>
              )}
            </div>
            <div className="text-xs text-slate-500">
              Auto-refreshes every 5 seconds
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <PhotoQueueFilters
            status={statusFilter}
            onStatusChange={setStatusFilter}
          />

          {isLoading ? (
            <div className="flex justify-center py-12">
              <Spinner size="lg" />
            </div>
          ) : (
            <PhotoQueueTable photos={photos || []} />
          )}
        </CardContent>
      </Card>
    </div>
  );
}

