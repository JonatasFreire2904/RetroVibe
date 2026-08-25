import type { ReactNode } from "react";

const VARIANTS = {
  violet: "bg-violet-100 text-violet-700",
  fuchsia: "bg-fuchsia-100 text-fuchsia-700",
  slate: "bg-slate-100 text-slate-600",
  cyan: "bg-cyan-100 text-cyan-700",
  emerald: "bg-emerald-100 text-emerald-700",
  amber: "bg-amber-100 text-amber-700",
  rose: "bg-rose-100 text-rose-700",
} as const;

interface BadgeProps {
  children: ReactNode;
  variant?: keyof typeof VARIANTS;
  className?: string;
}

export function Badge({ children, variant = "slate", className = "" }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-medium ${VARIANTS[variant]} ${className}`}
    >
      {children}
    </span>
  );
}
