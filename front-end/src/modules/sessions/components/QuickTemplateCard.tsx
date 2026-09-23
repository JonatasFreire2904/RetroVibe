import { useNavigate } from "react-router-dom";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import type { Template } from "@/shared/types";
import { ArrowRightIcon } from "@/shared/ui/Icons";
import { useCreateSessionMutation } from "../api/mutations";

export function QuickTemplateCard({ template }: { template: Template }) {
  const { data: user } = useCurrentUserQuery();
  const createSession = useCreateSessionMutation();
  const navigate = useNavigate();

  async function handleQuickStart() {
    const squadName = user?.squad ?? "Meu Squad";
    const session = await createSession.mutateAsync({ templateId: template.id, squadName, sequentialFlow: true });
    navigate(`/sessoes/${session.id}`);
  }

  return (
    <div className="group flex flex-col rounded-2xl border border-slate-100 bg-white p-5 shadow-sm transition-all duration-300 hover:-translate-y-1 hover:border-violet-200 hover:shadow-xl hover:shadow-violet-100/70 motion-reduce:transform-none">
      <div className="flex items-start justify-between">
        <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-cyan-50 text-cyan-500 transition-transform duration-300 group-hover:scale-110 group-hover:rotate-6 motion-reduce:transform-none">▶</span>
        <span className="text-xl transition-transform duration-300 group-hover:-rotate-12 group-hover:scale-125 motion-reduce:transform-none">{template.icon}</span>
      </div>
      <h3 className="mt-3 text-base font-bold text-slate-900">{template.label}</h3>
      <p className="mt-1 text-sm text-slate-500">{template.description}</p>

      <ul className="mt-4 space-y-2 text-sm text-slate-600">
        {template.columns.slice(0, 4).map((column) => (
          <li key={column.key} className="flex items-center gap-2">
            <span>{column.icon}</span>
            <span>{column.label}</span>
          </li>
        ))}
      </ul>

      <button
        onClick={handleQuickStart}
        disabled={createSession.isPending}
        className="mt-5 flex items-center gap-1 text-sm font-semibold text-fuchsia-600 transition hover:gap-2 disabled:opacity-50"
      >
        {createSession.isPending ? "Iniciando…" : "Usar este modelo"}
        <ArrowRightIcon width={14} height={14} />
      </button>
    </div>
  );
}
