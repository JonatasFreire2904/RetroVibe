interface ProgressBarProps {
  value: number;
  max: number;
  colorClassName?: string;
}

export function ProgressBar({ value, max, colorClassName = "bg-emerald-500" }: ProgressBarProps) {
  const percentage = max > 0 ? Math.min(100, Math.round((value / max) * 100)) : 0;
  return (
    <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
      <div className={`h-full rounded-full ${colorClassName}`} style={{ width: `${percentage}%` }} />
    </div>
  );
}
