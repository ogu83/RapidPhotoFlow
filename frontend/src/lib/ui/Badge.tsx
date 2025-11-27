import { ReactNode } from "react";

type BadgeVariant = "default" | "success" | "warning" | "error" | "info" | "processing";

interface BadgeProps {
  variant?: BadgeVariant;
  children: ReactNode;
  className?: string;
}

const variantClasses: Record<BadgeVariant, string> = {
  default: "bg-slate-700 text-slate-300",
  success: "bg-emerald-900/50 text-emerald-400 border border-emerald-700/50",
  warning: "bg-amber-900/50 text-amber-400 border border-amber-700/50",
  error: "bg-red-900/50 text-red-400 border border-red-700/50",
  info: "bg-blue-900/50 text-blue-400 border border-blue-700/50",
  processing: "bg-violet-900/50 text-violet-400 border border-violet-700/50",
};

export function Badge({ variant = "default", children, className = "" }: BadgeProps) {
  return (
    <span
      className={`
        inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
        ${variantClasses[variant]}
        ${className}
      `}
    >
      {children}
    </span>
  );
}

