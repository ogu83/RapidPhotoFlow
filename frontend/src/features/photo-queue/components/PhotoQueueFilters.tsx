import { PhotoStatus } from "../../../lib/types/photos";

interface PhotoQueueFiltersProps {
  status: PhotoStatus | "all";
  onStatusChange: (status: PhotoStatus | "all") => void;
}

const STATUS_OPTIONS: { value: PhotoStatus | "all"; label: string }[] = [
  { value: "all", label: "All Photos" },
  { value: "Queued", label: "Queued" },
  { value: "Processing", label: "Processing" },
  { value: "Processed", label: "Processed" },
  { value: "Failed", label: "Failed" },
];

export function PhotoQueueFilters({ status, onStatusChange }: PhotoQueueFiltersProps) {
  return (
    <div className="flex flex-wrap gap-2">
      {STATUS_OPTIONS.map((option) => (
        <button
          key={option.value}
          onClick={() => onStatusChange(option.value)}
          className={`
            px-3 py-1.5 text-sm font-medium rounded-lg transition-colors
            ${
              status === option.value
                ? "bg-primary-600/20 text-primary-400 border border-primary-600/50"
                : "bg-slate-800 text-slate-400 border border-slate-700 hover:text-slate-200 hover:border-slate-600"
            }
          `}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}

