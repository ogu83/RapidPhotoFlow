import { PhotoStatus } from "../types/photos";
import { Badge } from "./Badge";
import { Loader2, CheckCircle, XCircle, Clock, Upload } from "lucide-react";

interface PhotoStatusBadgeProps {
  status: PhotoStatus;
}

export function PhotoStatusBadge({ status }: PhotoStatusBadgeProps) {
  switch (status) {
    case "Uploaded":
      return (
        <Badge variant="default">
          <Upload className="w-3 h-3 mr-1" />
          Uploaded
        </Badge>
      );
    case "Queued":
      return (
        <Badge variant="info">
          <Clock className="w-3 h-3 mr-1" />
          Queued
        </Badge>
      );
    case "Processing":
      return (
        <Badge variant="processing">
          <Loader2 className="w-3 h-3 mr-1 animate-spin" />
          Processing
        </Badge>
      );
    case "Processed":
      return (
        <Badge variant="success">
          <CheckCircle className="w-3 h-3 mr-1" />
          Processed
        </Badge>
      );
    case "Failed":
      return (
        <Badge variant="error">
          <XCircle className="w-3 h-3 mr-1" />
          Failed
        </Badge>
      );
    default:
      return <Badge variant="default">{status}</Badge>;
  }
}

