import { useCallback, useState } from "react";
import { Upload, Image } from "lucide-react";

interface PhotoUploadDropzoneProps {
  onFilesSelected: (files: File[]) => void;
}

export function PhotoUploadDropzone({ onFilesSelected }: PhotoUploadDropzoneProps) {
  const [isDragging, setIsDragging] = useState(false);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);

      const files = Array.from(e.dataTransfer.files).filter((file) =>
        file.type.startsWith("image/")
      );

      if (files.length > 0) {
        onFilesSelected(files);
      }
    },
    [onFilesSelected]
  );

  const handleFileInput = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const files = Array.from(e.target.files || []);
      if (files.length > 0) {
        onFilesSelected(files);
      }
      // Reset input so the same file can be selected again
      e.target.value = "";
    },
    [onFilesSelected]
  );

  return (
    <div
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
      className={`
        relative border-2 border-dashed rounded-xl p-12 text-center transition-all cursor-pointer
        ${
          isDragging
            ? "border-primary-500 bg-primary-500/10"
            : "border-slate-700 hover:border-slate-600 hover:bg-slate-800/50"
        }
      `}
    >
      <input
        type="file"
        multiple
        accept="image/*"
        onChange={handleFileInput}
        className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
      />
      
      <div className="flex flex-col items-center gap-4">
        <div className={`
          p-4 rounded-full transition-colors
          ${isDragging ? "bg-primary-500/20" : "bg-slate-800"}
        `}>
          {isDragging ? (
            <Image className="w-10 h-10 text-primary-400" />
          ) : (
            <Upload className="w-10 h-10 text-slate-400" />
          )}
        </div>
        
        <div>
          <p className="text-lg font-medium text-slate-200">
            {isDragging ? "Drop your photos here" : "Drag & drop photos"}
          </p>
          <p className="text-sm text-slate-500 mt-1">
            or click to browse your files
          </p>
        </div>
        
        <p className="text-xs text-slate-600">
          Supports: JPG, PNG, GIF, WebP
        </p>
      </div>
    </div>
  );
}

