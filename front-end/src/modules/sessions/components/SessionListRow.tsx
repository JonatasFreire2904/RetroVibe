import { Link } from "react-router-dom";
import type { ReactNode } from "react";
import type { SessionSummary } from "@/shared/types";
import { formatDate } from "@/shared/lib/format";
import { Badge } from "@/shared/ui/Badge";
import { ArrowRightIcon, CheckSquareIcon, ClockIcon, UsersIcon } from "@/shared/ui/Icons";

export function SessionListRow({ session }: { session: SessionSummary }) {
  return (
    <Link
      to={`/sessoes/${session.id}`}
      className="flex flex-col gap-4 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm transition hover:border-violet-200 hover:shadow-md sm:flex-row sm:items-center sm:justify-between"
    >
      <div className="flex items-center gap-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-violet-50 text-xl">
          {session.theme.emoji}
        </div>
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-sm font-bold text-slate-900">{session.title || session.template.label}</p>
            <Badge variant="cyan">{session.theme.label}</Badge>
            <Badge variant="violet">{session.squad.name}</Badge>
          </div>
          <p className="mt-1 text-xs text-slate-400">{formatDate(session.date)}</p>
        </div>
      </div>

      <div className="flex items-center gap-6 text-xs text-slate-500">
        <Stat icon={<UsersIcon width={14} height={14} />} label="participantes" value={session.participantsCount} />
        <Stat icon={<CheckSquareIcon width={14} height={14} />} label="action items" value={session.actionItemsCount} />
        <Stat icon={<ClockIcon width={14} height={14} />} label="duração" value={`${session.durationMinutes} min`} mono />
        <span className="flex h-8 w-8 items-center justify-center rounded-full bg-emerald-50 text-emerald-500">
          <ArrowRightIcon width={16} height={16} />
        </span>
      </div>
    </Link>
  );
}

function Stat({
  icon,
  label,
  value,
  mono = false,
}: {
  icon: ReactNode;
  label: string;
  value: string | number;
  mono?: boolean;
}) {
  return (
    <div className="flex flex-col items-center gap-0.5">
      <span className={`flex items-center gap-1 font-semibold text-slate-700 ${mono ? "font-mono" : ""}`}>
        {icon}
        {value}
      </span>
      <span className="text-[10px] text-slate-400">{label}</span>
    </div>
  );
}
