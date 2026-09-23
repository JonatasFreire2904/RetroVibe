import type { ActionItemSessionSummary } from "@/shared/types";
import { formatDate } from "@/shared/lib/format";
import { ProgressBar } from "@/shared/ui/ProgressBar";

interface SessionSummaryCardProps {
  summary: ActionItemSessionSummary;
  active: boolean;
  onSelect: () => void;
}

export function SessionSummaryCard({ summary, active, onSelect }: SessionSummaryCardProps) {
  return (
    <button
      onClick={onSelect}
      className={`w-full rounded-xl border p-3 text-left transition ${
        active ? "border-violet-300 bg-violet-50" : "border-slate-100 bg-white hover:border-violet-200"
      }`}
    >
      <div className="flex items-center justify-between">
        <span className="flex items-center gap-2 text-sm font-semibold text-slate-800">
          <span>{summary.themeIcon}</span>
          {summary.templateLabel}
        </span>
        <span className="font-mono text-xs text-violet-500">{summary.totalItems}</span>
      </div>
      <p className="mt-1 text-xs text-slate-400">{formatDate(summary.date)}</p>
      <div className="mt-2">
        <ProgressBar value={summary.completedItems} max={summary.totalItems} />
        <p className="mt-1 text-[11px] text-slate-400">
          {summary.completedItems}/{summary.totalItems} concluídos
        </p>
      </div>
    </button>
  );
}
