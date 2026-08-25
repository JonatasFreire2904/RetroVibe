interface BarRowProps {
  label: string;
  value: number;
  max: number;
  valueLabel: string;
  colorClassName?: string;
}

export function BarRow({ label, value, max, valueLabel, colorClassName = "bg-cyan-400" }: BarRowProps) {
  const percentage = max > 0 ? Math.round((value / max) * 100) : 0;
  return (
    <div>
      <div className="mb-1 flex items-center justify-between text-sm">
        <span className="text-slate-600">{label}</span>
        <span className="font-mono text-xs font-semibold text-slate-500">{valueLabel}</span>
      </div>
      <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
        <div className={`h-full rounded-full ${colorClassName}`} style={{ width: `${percentage}%` }} />
      </div>
    </div>
  );
}
