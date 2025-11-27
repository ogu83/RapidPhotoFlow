import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Upload, CheckCircle } from "lucide-react";
import { Card, CardContent, CardHeader } from "../../lib/ui/Card";
import { Button } from "../../lib/ui/Button";
import { PhotoUploadDropzone } from "./components/PhotoUploadDropzone";
import { PhotoUploadList } from "./components/PhotoUploadList";
import { usePhotoUpload } from "./hooks/usePhotoUpload";

export function PhotoUpload() {
  const [files, setFiles] = useState<File[]>([]);
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const uploadMutation = usePhotoUpload();
  const navigate = useNavigate();

  const handleFilesSelected = (newFiles: File[]) => {
    setFiles((prev) => [...prev, ...newFiles]);
    setUploadSuccess(false);
  };

  const handleRemoveFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleClearAll = () => {
    setFiles([]);
    setUploadSuccess(false);
  };

  const handleUpload = async () => {
    if (files.length === 0) return;

    try {
      await uploadMutation.mutateAsync(files);
      setUploadSuccess(true);
      setFiles([]);
    } catch (error) {
      console.error("Upload failed:", error);
    }
  };

  const handleGoToQueue = () => {
    navigate("/queue");
  };

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-100">Upload Photos</h1>
        <p className="text-slate-400 mt-1">
          Upload your photos for processing
        </p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <Upload className="w-5 h-5 text-primary-400" />
            <h2 className="text-lg font-semibold text-slate-100">
              Select Photos
            </h2>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <PhotoUploadDropzone onFilesSelected={handleFilesSelected} />

          <PhotoUploadList files={files} onRemove={handleRemoveFile} />

          {uploadSuccess && (
            <div className="flex items-center gap-2 p-4 bg-emerald-900/20 border border-emerald-700/50 rounded-lg">
              <CheckCircle className="w-5 h-5 text-emerald-400" />
              <p className="text-emerald-400">
                Photos uploaded successfully! They are now being processed.
              </p>
            </div>
          )}

          {uploadMutation.isError && (
            <div className="p-4 bg-red-900/20 border border-red-700/50 rounded-lg">
              <p className="text-red-400">
                Upload failed. Please try again.
              </p>
            </div>
          )}

          <div className="flex items-center justify-between pt-4 border-t border-slate-700">
            <div className="flex gap-2">
              {files.length > 0 && (
                <Button variant="ghost" onClick={handleClearAll}>
                  Clear All
                </Button>
              )}
            </div>
            <div className="flex gap-2">
              {uploadSuccess && (
                <Button variant="secondary" onClick={handleGoToQueue}>
                  View Queue
                </Button>
              )}
              <Button
                onClick={handleUpload}
                disabled={files.length === 0}
                isLoading={uploadMutation.isPending}
              >
                <Upload className="w-4 h-4 mr-2" />
                Upload {files.length > 0 && `(${files.length})`}
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

