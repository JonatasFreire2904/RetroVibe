import { useState } from "react";
import {
  useCreateTemplateMutation,
  useCreateThemeMutation,
  useUpdateTemplateMutation,
  useUpdateThemeMutation,
} from "@/modules/catalog/api/adminMutations";
import { useAdminTemplatesQuery, useAdminThemesQuery } from "@/modules/catalog/api/adminQueries";
import type { Template, Theme } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { Card } from "@/shared/ui/Card";
import { PlusIcon } from "@/shared/ui/Icons";

export function AdminCatalogPage() {
  const { data: templates = [] } = useAdminTemplatesQuery();
  const { data: themes = [] } = useAdminThemesQuery();

  return (
    <div className="mx-auto max-w-5xl">
      <h1 className="text-2xl font-extrabold text-slate-900">Catálogo</h1>
      <p className="mt-1 text-sm text-slate-500">
        Gerencie modelos e temas disponíveis para novas sessões — desativar não afeta sessões já criadas.
      </p>

      <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-2">
        <ThemesSection themes={themes} />
        <TemplatesSection templates={templates} />
      </div>
    </div>
  );
}

function ThemesSection({ themes }: { themes: Theme[] }) {
  const [label, setLabel] = useState("");
  const [emoji, setEmoji] = useState("");
  const createTheme = useCreateThemeMutation();
  const updateTheme = useUpdateThemeMutation();

  function handleCreate() {
    if (!label.trim() || !emoji.trim()) return;
    createTheme.mutate(
      { label: label.trim(), emoji: emoji.trim() },
      { onSuccess: () => { setLabel(""); setEmoji(""); } }
    );
  }

  return (
    <Card className="p-5">
      <h2 className="mb-4 text-sm font-bold text-slate-800">Temas visuais</h2>
      <div className="space-y-2">
        {themes.map((theme) => (
          <div key={theme.id} className="flex items-center justify-between rounded-xl border border-slate-100 p-3">
            <span className="flex items-center gap-2 text-sm text-slate-700">
              <span>{theme.emoji}</span>
              {theme.label}
              {!theme.active && <span className="text-xs text-slate-400">(inativo)</span>}
            </span>
            <Button
              size="sm"
              variant={theme.active ? "outline" : "primary"}
              disabled={updateTheme.isPending}
              onClick={() => updateTheme.mutate({ id: theme.id, active: !theme.active })}
            >
              {theme.active ? "Desativar" : "Ativar"}
            </Button>
          </div>
        ))}
      </div>

      <div className="mt-4 flex items-center gap-2 border-t border-slate-100 pt-4">
        <input
          value={emoji}
          onChange={(e) => setEmoji(e.target.value)}
          placeholder="🎭"
          maxLength={4}
          className="w-14 rounded-lg border border-slate-200 px-2 py-2 text-center text-sm outline-none focus:border-violet-400"
        />
        <input
          value={label}
          onChange={(e) => setLabel(e.target.value)}
          placeholder="Nome do novo tema..."
          className="flex-1 rounded-lg border border-slate-200 px-3 py-2 text-sm outline-none focus:border-violet-400"
        />
        <Button size="sm" icon={<PlusIcon width={14} height={14} />} disabled={createTheme.isPending} onClick={handleCreate}>
          Adicionar
        </Button>
      </div>
    </Card>
  );
}

function TemplatesSection({ templates }: { templates: Template[] }) {
  const [label, setLabel] = useState("");
  const [icon, setIcon] = useState("");
  const createTemplate = useCreateTemplateMutation();
  const updateTemplate = useUpdateTemplateMutation();

  function handleCreate() {
    if (!label.trim() || !icon.trim()) return;
    createTemplate.mutate(
      {
        label: label.trim(),
        icon: icon.trim(),
        description: "",
        columns: [{ key: "itens", label: "Itens", icon: "📝" }],
      },
      { onSuccess: () => { setLabel(""); setIcon(""); } }
    );
  }

  return (
    <Card className="p-5">
      <h2 className="mb-4 text-sm font-bold text-slate-800">Modelos de retrospectiva</h2>
      <div className="space-y-2">
        {templates.map((template) => (
          <div key={template.id} className="flex items-center justify-between rounded-xl border border-slate-100 p-3">
            <span className="flex items-center gap-2 text-sm text-slate-700">
              <span>{template.icon}</span>
              {template.label}
              {!template.active && <span className="text-xs text-slate-400">(inativo)</span>}
            </span>
            <Button
              size="sm"
              variant={template.active ? "outline" : "primary"}
              disabled={updateTemplate.isPending}
              onClick={() => updateTemplate.mutate({ id: template.id, active: !template.active })}
            >
              {template.active ? "Desativar" : "Ativar"}
            </Button>
          </div>
        ))}
      </div>

      <div className="mt-4 flex items-center gap-2 border-t border-slate-100 pt-4">
        <input
          value={icon}
          onChange={(e) => setIcon(e.target.value)}
          placeholder="🧩"
          maxLength={4}
          className="w-14 rounded-lg border border-slate-200 px-2 py-2 text-center text-sm outline-none focus:border-violet-400"
        />
        <input
          value={label}
          onChange={(e) => setLabel(e.target.value)}
          placeholder="Nome do novo modelo..."
          className="flex-1 rounded-lg border border-slate-200 px-3 py-2 text-sm outline-none focus:border-violet-400"
        />
        <Button size="sm" icon={<PlusIcon width={14} height={14} />} disabled={createTemplate.isPending} onClick={handleCreate}>
          Adicionar
        </Button>
      </div>
      <p className="mt-2 text-xs text-slate-400">Novos modelos nascem com uma coluna genérica "Itens".</p>
    </Card>
  );
}
