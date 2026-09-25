import { useState } from "react";
import { useSquadsQuery, useTemplatesQuery, useThemesQuery } from "@/modules/catalog/api/queries";
import { useSessionsQuery } from "@/modules/sessions/api/queries";
import { SessionListRow } from "@/modules/sessions/components/SessionListRow";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { SearchIcon } from "@/shared/ui/Icons";

export function SessionHistoryPage() {
  const [search, setSearch] = useState("");
  const [squadId, setSquadId] = useState("");
  const [templateId, setTemplateId] = useState("");
  const [themeId, setThemeId] = useState("");

  const { data: currentUser } = useCurrentUserQuery();
  const isAdmin = currentUser?.accessLevel === "ADMIN";

  const { data: squads = [] } = useSquadsQuery();
  const { data: templates = [] } = useTemplatesQuery();
  const { data: themes = [] } = useThemesQuery();

  const { data: sessions = [], isLoading } = useSessionsQuery({
    search: search || undefined,
    squadId: squadId || undefined,
    templateId: templateId || undefined,
    themeId: themeId || undefined,
  });

  return (
    <div className="mx-auto max-w-5xl">
      <div className="mb-6 flex items-baseline justify-between">
        <div>
          <h1 className="text-2xl font-extrabold text-slate-900">Histórico de Sessões</h1>
          <p className="mt-1 text-sm text-slate-500">
            {isLoading ? "Carregando…" : `${sessions.length} sessões encontradas`}
          </p>
        </div>
      </div>

      <div className="mb-6 flex flex-col gap-3 rounded-2xl border border-slate-100 bg-white p-3 shadow-sm sm:flex-row">
        <div className="relative flex-1">
          <SearchIcon className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Buscar por modelo, tema ou squad..."
            className="w-full rounded-xl border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm outline-none placeholder:text-slate-400 focus:border-violet-400"
          />
        </div>
        {(isAdmin || squads.length > 1) && (
          <FilterSelect
            value={squadId}
            onChange={setSquadId}
            allLabel="Todos os Squads"
            options={squads.map((s) => ({ value: s.id, label: s.name }))}
          />
        )}
        <FilterSelect
          value={templateId}
          onChange={setTemplateId}
          allLabel="Todos os Modelos"
          options={templates.map((t) => ({ value: t.id, label: t.label }))}
        />
        <FilterSelect
          value={themeId}
          onChange={setThemeId}
          allLabel="Todos os Temas"
          options={themes.map((t) => ({ value: t.id, label: t.label }))}
        />
      </div>

      <div className="space-y-3">
        {sessions.length === 0 && !isLoading && (
          <p className="rounded-2xl border border-dashed border-slate-200 p-8 text-center text-sm text-slate-400">
            Nenhuma sessão encontrada com esses filtros.
          </p>
        )}
        {sessions.map((session) => (
          <SessionListRow key={session.id} session={session} />
        ))}
      </div>
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
      className="rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-600 outline-none focus:border-violet-400"
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
