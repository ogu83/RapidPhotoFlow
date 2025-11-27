import { PhotoDto } from "../../../lib/types/photos";
import { PhotoStatusBadge } from "../../../lib/ui/PhotoStatusBadge";
import { formatFileSize } from "../../../lib/utils/formatFileSize";
import { formatDate } from "../../../lib/utils/formatDate";
import { FileImage, Calendar, HardDrive, Clock } from "lucide-react";

interface PhotoDetailHeaderProps {
  photo: PhotoDto;
}

export function PhotoDetailHeader({ photo }: PhotoDetailHeaderProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-100 flex items-center gap-3">
            <FileImage className="w-7 h-7 text-primary-400" />
            {photo.fileName}
          </h1>
          <p className="text-slate-500 mt-1">{photo.contentType}</p>
        </div>
        <PhotoStatusBadge status={photo.status} />
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-slate-800/50 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center gap-2 text-slate-400 mb-1">
            <HardDrive className="w-4 h-4" />
            <span className="text-xs font-medium">Size</span>
          </div>
          <p className="text-lg font-semibold text-slate-200">
            {formatFileSize(photo.sizeBytes)}
          </p>
        </div>

        <div className="bg-slate-800/50 rounded-lg p-4 border border-slate-700">
          <div className="flex items-center gap-2 text-slate-400 mb-1">
            <Calendar className="w-4 h-4" />
            <span className="text-xs font-medium">Uploaded</span>
          </div>
          <p className="text-sm font-medium text-slate-200">
            {formatDate(photo.uploadedAt)}
          </p>
        </div>

        {photo.processingStartedAt && (
          <div className="bg-slate-800/50 rounded-lg p-4 border border-slate-700">
            <div className="flex items-center gap-2 text-slate-400 mb-1">
              <Clock className="w-4 h-4" />
              <span className="text-xs font-medium">Processing Started</span>
            </div>
            <p className="text-sm font-medium text-slate-200">
              {formatDate(photo.processingStartedAt)}
            </p>
          </div>
        )}

        {photo.processingCompletedAt && (
          <div className="bg-slate-800/50 rounded-lg p-4 border border-slate-700">
            <div className="flex items-center gap-2 text-slate-400 mb-1">
              <Clock className="w-4 h-4" />
              <span className="text-xs font-medium">Completed</span>
            </div>
            <p className="text-sm font-medium text-slate-200">
              {formatDate(photo.processingCompletedAt)}
            </p>
          </div>
        )}
      </div>

      {photo.errorMessage && (
        <div className="bg-red-900/20 border border-red-700/50 rounded-lg p-4">
          <p className="text-sm font-medium text-red-400">Error</p>
          <p className="text-red-300 mt-1">{photo.errorMessage}</p>
        </div>
      )}
    </div>
  );
}

