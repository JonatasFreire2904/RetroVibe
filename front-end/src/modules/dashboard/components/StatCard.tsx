import type { ReactNode } from "react";

interface StatCardProps {
  icon: ReactNode;
  value: string;
  label: string;
}

export function StatCard({ icon, value, label }: StatCardProps) {
  return (
    <div className="flex items-center gap-3 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-violet-50 text-violet-500">{icon}</span>
      <div>
        <p className="font-mono text-lg font-bold text-slate-900">{value}</p>
        <p className="text-xs text-slate-400">{label}</p>
      </div>
    </div>
  );
}
