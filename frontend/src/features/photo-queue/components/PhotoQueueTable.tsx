import { Link } from "react-router-dom";
import { Eye } from "lucide-react";
import { PhotoListItemDto } from "../../../lib/types/photos";
import { PhotoStatusBadge } from "../../../lib/ui/PhotoStatusBadge";
import { formatRelativeTime } from "../../../lib/utils/formatDate";

interface PhotoQueueTableProps {
  photos: PhotoListItemDto[];
}

export function PhotoQueueTable({ photos }: PhotoQueueTableProps) {
  if (photos.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-slate-400">No photos found</p>
      </div>
    );
  }

  return (
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
                <Link
                  to={`/photos/${photo.id}`}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-primary-400 hover:text-primary-300 hover:bg-primary-500/10 rounded-lg transition-colors"
                >
                  <Eye className="w-4 h-4" />
                  View
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

