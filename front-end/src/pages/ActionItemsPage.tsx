import { useEffect, useState } from "react";
import { useSquadsQuery, useTemplatesQuery, useThemesQuery } from "@/modules/catalog/api/queries";
import { useCreateActionItemMutation } from "@/modules/action-items/api/mutations";
import { useActionItemSessionsSummaryQuery, useActionItemsQuery } from "@/modules/action-items/api/queries";
import { ActionItemRow } from "@/modules/action-items/components/ActionItemRow";
import { SessionSummaryCard } from "@/modules/action-items/components/SessionSummaryCard";
import { useSessionBoardQuery } from "@/modules/sessions/api/queries";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import type { ActionItemStatus } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { PlusIcon } from "@/shared/ui/Icons";

const STATUS_TABS: { value: ActionItemStatus | "ALL"; label: string }[] = [
  { value: "ALL", label: "Todos" },
  { value: "PLANNED", label: "Planejado" },
  { value: "BACKLOG", label: "Backlog" },
  { value: "IN_PROGRESS", label: "Em andamento" },
  { value: "DONE", label: "Concluído" },
  { value: "DISCARDED", label: "Descartado" },
];

export function ActionItemsPage() {
  const [squadId, setSquadId] = useState("");
  const [templateId, setTemplateId] = useState("");
  const [themeId, setThemeId] = useState("");
  const [selectedSessionId, setSelectedSessionId] = useState<string | null>(null);
  const [statusTab, setStatusTab] = useState<ActionItemStatus | "ALL">("ALL");
  const [draft, setDraft] = useState("");

  const { data: currentUser } = useCurrentUserQuery();
  const isAdmin = currentUser?.accessLevel === "ADMIN";

  const { data: squads = [] } = useSquadsQuery();
  const { data: templates = [] } = useTemplatesQuery();
  const { data: themes = [] } = useThemesQuery();

  const filters = {
    squadId: squadId || undefined,
    templateId: templateId || undefined,
    themeId: themeId || undefined,
  };

  const { data: sessionSummaries = [] } = useActionItemSessionsSummaryQuery(filters);

  useEffect(() => {
    if (sessionSummaries.length === 0) {
      setSelectedSessionId(null);
      return;
    }
    if (!selectedSessionId || !sessionSummaries.some((s) => s.sessionId === selectedSessionId)) {
      setSelectedSessionId(sessionSummaries[0].sessionId);
    }
  }, [sessionSummaries, selectedSessionId]);

  const { data: board } = useSessionBoardQuery(selectedSessionId ?? undefined);
  const { data: items = [] } = useActionItemsQuery({ sessionId: selectedSessionId ?? undefined });
  const createItem = useCreateActionItemMutation();

  const counts = STATUS_TABS.reduce<Record<string, number>>((acc, tab) => {
    acc[tab.value] = tab.value === "ALL" ? items.length : items.filter((i) => i.status === tab.value).length;
    return acc;
  }, {});

  const visibleItems = statusTab === "ALL" ? items : items.filter((i) => i.status === statusTab);

  function handleAddItem() {
    const description = draft.trim();
    if (!description || !selectedSessionId) return;
    createItem.mutate(
      { sessionId: selectedSessionId, description },
      { onSuccess: () => setDraft("") }
    );
  }

  return (
    <div className="mx-auto grid max-w-6xl grid-cols-1 gap-6 lg:grid-cols-[280px_1fr]">
      <aside className="space-y-4">
        <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
          <p className="mb-3 text-xs font-bold uppercase tracking-wide text-slate-400">Filtrar sessões</p>
          <div className="space-y-2">
            {isAdmin && (
              <FilterSelect value={squadId} onChange={setSquadId} allLabel="Squad" options={squads.map((s) => ({ value: s.id, label: s.name }))} />
            )}
            <FilterSelect value={templateId} onChange={setTemplateId} allLabel="Modelo" options={templates.map((t) => ({ value: t.id, label: t.label }))} />
            <FilterSelect value={themeId} onChange={setThemeId} allLabel="Tema" options={themes.map((t) => ({ value: t.id, label: t.label }))} />
          </div>
        </div>

        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-wide text-slate-400">Sessões</p>
          <div className="space-y-2">
            {sessionSummaries.map((summary) => (
              <SessionSummaryCard
                key={summary.sessionId}
                summary={summary}
                active={summary.sessionId === selectedSessionId}
                onSelect={() => setSelectedSessionId(summary.sessionId)}
              />
            ))}
            {sessionSummaries.length === 0 && <p className="text-xs text-slate-400">Nenhuma sessão com itens de ação.</p>}
          </div>
        </div>
      </aside>

      <section>
        {board ? (
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-2">
              <span className="text-xl">{board.theme.emoji}</span>
              <div>
                <h1 className="text-lg font-extrabold text-slate-900">{board.template.label}</h1>
                <p className="text-xs text-slate-400">Squad {board.squad.name}</p>
              </div>
            </div>
            <Button size="sm" icon={<PlusIcon width={14} height={14} />} onClick={() => document.getElementById("new-item-input")?.focus()}>
              Novo item
            </Button>
          </div>
        ) : (
          <p className="mb-4 text-sm text-slate-400">Selecione uma sessão para ver os itens de ação.</p>
        )}

        <div className="mb-4 flex flex-wrap gap-2">
          {STATUS_TABS.map((tab) => (
            <button
              key={tab.value}
              onClick={() => setStatusTab(tab.value)}
              className={`rounded-full px-3 py-1.5 text-xs font-semibold transition ${
                statusTab === tab.value ? "bg-violet-600 text-white" : "bg-white text-slate-500 hover:bg-slate-100"
              }`}
            >
              {tab.label} {counts[tab.value] ?? 0}
            </button>
          ))}
        </div>

        {selectedSessionId && (
          <div className="mb-4 flex items-center gap-2 rounded-xl border border-slate-100 bg-white p-3 shadow-sm">
            <input
              id="new-item-input"
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleAddItem()}
              placeholder="Descreva o item de ação..."
              className="flex-1 rounded-lg border-none bg-transparent px-2 py-1.5 text-sm outline-none placeholder:text-slate-400"
            />
            <Button size="sm" disabled={!draft.trim() || createItem.isPending} onClick={handleAddItem}>
              Adicionar
            </Button>
          </div>
        )}

        <div className="space-y-3">
          {visibleItems.map((item) => (
            <ActionItemRow key={item.id} item={item} />
          ))}
          {selectedSessionId && visibleItems.length === 0 && (
            <p className="rounded-xl border border-dashed border-slate-200 p-6 text-center text-sm text-slate-400">
              Nenhum item nesse status.
            </p>
          )}
        </div>
      </section>
    </div>
  );
}

function FilterSelect({
  value,
  onChange,
  allLabel,
  options,
}: {
  value: string;
  onChange: (value: string) => void;
  allLabel: string;
  options: { value: string; label: string }[];
}) {
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-600 outline-none focus:border-violet-400"
    >
      <option value="">{allLabel}</option>
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  );
}
