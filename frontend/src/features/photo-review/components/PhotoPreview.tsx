import { useState } from "react";
import { Image, ZoomIn, ZoomOut, X } from "lucide-react";
import { PhotoDto } from "../../../lib/types/photos";

interface PhotoPreviewProps {
  photo: PhotoDto;
}

// In Docker, VITE_API_BASE_URL is empty string - nginx proxies /api/* to backend
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:7001";

export function PhotoPreview({ photo }: PhotoPreviewProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [hasError, setHasError] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);

  const photoUrl = `${API_BASE_URL}/api/photos/${photo.id}/file`;

  // Only show preview for processed photos
  if (photo.status !== "Processed") {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-slate-500">
        <Image className="w-16 h-16 mb-4 opacity-50" />
        <p className="text-sm">
          Preview available after processing completes
        </p>
      </div>
    );
  }

  if (hasError) {
    return (
      <div className="flex flex-col items-center justify-center py-12 text-slate-500">
        <Image className="w-16 h-16 mb-4 opacity-50" />
        <p className="text-sm">Unable to load photo preview</p>
      </div>
    );
  }

  return (
    <>
      <div className="relative">
        {isLoading && (
          <div className="absolute inset-0 flex items-center justify-center bg-slate-800/50 rounded-lg">
            <div className="w-8 h-8 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}
        <img
          src={photoUrl}
          alt={photo.fileName}
          className={`
            w-full max-h-96 object-contain rounded-lg bg-slate-900/50 cursor-pointer
            transition-opacity duration-300
            ${isLoading ? "opacity-0" : "opacity-100"}
          `}
          onLoad={() => setIsLoading(false)}
          onError={() => {
            setIsLoading(false);
            setHasError(true);
          }}
          onClick={() => setIsFullscreen(true)}
        />
        {!isLoading && (
          <button
            onClick={() => setIsFullscreen(true)}
            className="absolute bottom-3 right-3 p-2 bg-slate-900/80 hover:bg-slate-800 rounded-lg transition-colors"
            title="View fullscreen"
          >
            <ZoomIn className="w-5 h-5 text-slate-300" />
          </button>
        )}
      </div>

      {/* Fullscreen Modal */}
      {isFullscreen && (
        <div
          className="fixed inset-0 z-50 bg-black/95 flex items-center justify-center"
          onClick={() => setIsFullscreen(false)}
        >
          <button
            onClick={() => setIsFullscreen(false)}
            className="absolute top-4 right-4 p-2 bg-slate-800/80 hover:bg-slate-700 rounded-lg transition-colors"
          >
            <X className="w-6 h-6 text-white" />
          </button>
          <button
            onClick={() => setIsFullscreen(false)}
            className="absolute top-4 left-4 p-2 bg-slate-800/80 hover:bg-slate-700 rounded-lg transition-colors"
          >
            <ZoomOut className="w-6 h-6 text-white" />
          </button>
          <img
            src={photoUrl}
            alt={photo.fileName}
            className="max-w-[95vw] max-h-[95vh] object-contain"
            onClick={(e) => e.stopPropagation()}
          />
          <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-slate-900/80 px-4 py-2 rounded-lg">
            <p className="text-sm text-slate-300">{photo.fileName}</p>
          </div>
        </div>
      )}
    </>
  );
}

