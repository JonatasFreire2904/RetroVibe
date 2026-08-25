import { Link } from "react-router-dom";
import type { SessionSummary } from "@/shared/types";
import { formatDate } from "@/shared/lib/format";
import { ArrowRightIcon, UsersIcon } from "@/shared/ui/Icons";

export function RecentSessionCard({ session }: { session: SessionSummary }) {
  return (
    <Link
      to={`/sessoes/${session.id}`}
      className="group flex items-center justify-between rounded-2xl border border-slate-100 bg-white p-5 shadow-sm transition hover:border-violet-200 hover:shadow-md"
    >
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-violet-50 text-lg">
          {session.template.icon}
        </div>
        <div>
          <p className="text-sm font-semibold text-slate-800">{session.title || session.template.label}</p>
          <p className="mt-1 flex items-center gap-2 text-xs text-slate-400">
            <span>{formatDate(session.date)}</span>
            <span className="flex items-center gap-1">
              <UsersIcon width={12} height={12} />
              {session.participantsCount} pessoas
            </span>
          </p>
        </div>
      </div>
      <span className="flex h-8 w-8 items-center justify-center rounded-full bg-violet-50 text-violet-500 transition group-hover:bg-violet-100">
        <ArrowRightIcon width={16} height={16} />
      </span>
    </Link>
  );
}
