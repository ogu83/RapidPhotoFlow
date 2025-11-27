import { X, Image } from "lucide-react";
import { formatFileSize } from "../../../lib/utils/formatFileSize";

interface PhotoUploadListProps {
  files: File[];
  onRemove: (index: number) => void;
}

export function PhotoUploadList({ files, onRemove }: PhotoUploadListProps) {
  if (files.length === 0) return null;

  return (
    <div className="space-y-2">
      <p className="text-sm font-medium text-slate-400">
        {files.length} file{files.length !== 1 ? "s" : ""} selected
      </p>
      
      <div className="grid gap-2">
        {files.map((file, index) => (
          <div
            key={`${file.name}-${index}`}
            className="flex items-center gap-3 p-3 bg-slate-800/50 rounded-lg border border-slate-700"
          >
            <div className="p-2 bg-slate-700 rounded-lg">
              <Image className="w-5 h-5 text-slate-400" />
            </div>
            
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-slate-200 truncate">
                {file.name}
              </p>
              <p className="text-xs text-slate-500">
                {formatFileSize(file.size)}
              </p>
            </div>
            
            <button
              onClick={() => onRemove(index)}
              className="p-1.5 text-slate-500 hover:text-red-400 hover:bg-red-500/10 rounded-lg transition-colors"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

