import { PhotoEventDto } from "../../../lib/types/photos";
import { formatDate } from "../../../lib/utils/formatDate";
import {
  Upload,
  Clock,
  Play,
  CheckCircle,
  XCircle,
  Circle,
} from "lucide-react";

interface PhotoEventTimelineProps {
  events: PhotoEventDto[];
}

function getEventIcon(eventType: string) {
  switch (eventType.toLowerCase()) {
    case "uploaded":
      return <Upload className="w-4 h-4" />;
    case "queued":
      return <Clock className="w-4 h-4" />;
    case "processingstarted":
      return <Play className="w-4 h-4" />;
    case "processingcompleted":
      return <CheckCircle className="w-4 h-4" />;
    case "processingfailed":
      return <XCircle className="w-4 h-4" />;
    default:
      return <Circle className="w-4 h-4" />;
  }
}

function getEventColor(eventType: string) {
  switch (eventType.toLowerCase()) {
    case "uploaded":
      return "bg-slate-600 text-slate-300";
    case "queued":
      return "bg-blue-600 text-blue-300";
    case "processingstarted":
      return "bg-violet-600 text-violet-300";
    case "processingcompleted":
      return "bg-emerald-600 text-emerald-300";
    case "processingfailed":
      return "bg-red-600 text-red-300";
    default:
      return "bg-slate-600 text-slate-300";
  }
}

export function PhotoEventTimeline({ events }: PhotoEventTimelineProps) {
  if (events.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-slate-400">No events recorded</p>
      </div>
    );
  }

  return (
    <div className="relative">
      {/* Timeline line */}
      <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-slate-700" />

      <div className="space-y-4">
        {events.map((event, index) => (
          <div key={event.id} className="relative flex gap-4 pl-2">
            {/* Icon */}
            <div
              className={`
                relative z-10 flex items-center justify-center w-8 h-8 rounded-full
                ${getEventColor(event.eventType)}
              `}
            >
              {getEventIcon(event.eventType)}
            </div>

            {/* Content */}
            <div className={`flex-1 pb-4 ${index === events.length - 1 ? "" : ""}`}>
              <div className="bg-slate-800/50 rounded-lg p-4 border border-slate-700">
                <div className="flex items-center justify-between mb-1">
                  <span className="text-sm font-semibold text-slate-200">
                    {event.eventType}
                  </span>
                  <span className="text-xs text-slate-500">
                    {formatDate(event.createdAt)}
                  </span>
                </div>
                <p className="text-sm text-slate-400">{event.message}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

