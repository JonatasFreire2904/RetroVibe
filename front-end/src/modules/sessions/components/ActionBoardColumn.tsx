import { useState, type CSSProperties } from "react";
import { useCreateActionItemMutation, useUpdateActionItemMutation } from "@/modules/action-items/api/mutations";
import type { ActionItem, ActionItemStatus } from "@/shared/types";
import type { RetroPalette, RetroTheme } from "../retroTheme";
import { ThemeTrim } from "./ThemeTrim";

const statuses: { value: ActionItemStatus; label: string }[] = [
  { value: "PLANNED", label: "Planejado" },
  { value: "BACKLOG", label: "Backlog" },
  { value: "IN_PROGRESS", label: "Em andamento" },
  { value: "DONE", label: "Concluído" },
  { value: "DISCARDED", label: "Descartado" },
];

interface Props {
  sessionId: string;
  theme: RetroTheme;
  palette: RetroPalette;
  items: ActionItem[];
  assignees: { id: string; name: string; avatarColor: string }[];
  isFacilitator: boolean;
  isActive: boolean;
  isClosing?: boolean;
  onClose?: () => void;
}

function ActionBoardCard({ item, assignees, canEdit }: {
  item: ActionItem;
  assignees: Props["assignees"];
  canEdit: boolean;
}) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(item.description);
  const updateItem = useUpdateActionItemMutation();

  function save() {
    const description = draft.trim();
    if (!description) return;
    if (description !== item.description) {
      updateItem.mutate({ id: item.id, description }, { onSuccess: () => setEditing(false) });
    } else {
      setEditing(false);
    }
  }

  return <div className="retro-action-card">
    {editing ? <div className="retro-action-edit">
      <textarea autoFocus value={draft} maxLength={280} onChange={event => setDraft(event.target.value)} rows={3} />
      <div>
        <button type="button" disabled={!draft.trim() || updateItem.isPending} onClick={save}>Salvar</button>
        <button type="button" onClick={() => { setDraft(item.description); setEditing(false); }}>Cancelar</button>
      </div>
    </div> : <div className="retro-action-card-title">
      <p>{item.description}</p>
      {canEdit && <button type="button" onClick={() => setEditing(true)} aria-label="Editar item de ação">✎</button>}
    </div>}
    <div className="retro-action-card-fields">
      {canEdit ? <>
        <select aria-label={`Status de ${item.description}`} value={item.status}
          onChange={event => updateItem.mutate({ id: item.id, status: event.target.value as ActionItemStatus })}>
          {statuses.map(status => <option key={status.value} value={status.value}>{status.label}</option>)}
        </select>
        <select aria-label={`Responsável por ${item.description}`} value={item.assignee?.id ?? ""}
          onChange={event => updateItem.mutate({ id: item.id, assigneeId: event.target.value || null })}>
          <option value="">Sem responsável</option>
          {assignees.map(user => <option key={user.id} value={user.id}>{user.name}</option>)}
          {item.assignee && !assignees.some(user => user.id === item.assignee?.id) &&
            <option value={item.assignee.id}>{item.assignee.name}</option>}
        </select>
      </> : <>
        <span>{statuses.find(status => status.value === item.status)?.label}</span>
        <span>{item.assignee?.name ?? "Sem responsável"}</span>
      </>}
    </div>
  </div>;
}

export function ActionBoardColumn({ sessionId, theme, palette, items, assignees, isFacilitator, isActive,
  isClosing = false, onClose }: Props) {
  const [draft, setDraft] = useState("");
  const createActionItem = useCreateActionItemMutation();
  const style = {
    "--retro-column-bg": palette.background,
    "--retro-column-border": palette.border,
    "--retro-column-accent": palette.accent,
    "--retro-card-bg": palette.card,
  } as CSSProperties;

  return <article className="retro-column retro-action-column" style={style}>
    <ThemeTrim decor={theme.decor} small />
    <header className="retro-column-heading">
      <div className="retro-column-kicker">
        <span className="retro-column-icon">📌</span>
        <span className="retro-column-count">{items.length}</span>
      </div>
      <h2>Itens de Ação</h2>
      <p>Compromissos para a equipe acompanhar</p>
      <div className="retro-column-rule" />
    </header>
    <div className="retro-column-content">
      {isFacilitator && isActive && <form className="retro-action-compose" onSubmit={event => {
        event.preventDefault();
        const description = draft.trim();
        if (!description) return;
        createActionItem.mutate({ sessionId, description }, { onSuccess: () => setDraft("") });
      }}>
        <textarea value={draft} onChange={event => setDraft(event.target.value)} maxLength={280} rows={3}
          placeholder="Qual compromisso o time vai assumir?" aria-label="Novo item de ação" />
        <button type="submit" disabled={!draft.trim() || createActionItem.isPending}>＋ Adicionar ação</button>
        {createActionItem.isError && <p role="alert">Não foi possível criar o item de ação.</p>}
      </form>}
      {items.map(item => <ActionBoardCard key={item.id} item={item} assignees={assignees}
        canEdit={isFacilitator && isActive} />)}
      {items.length === 0 && <p className="retro-action-empty">Ainda não há itens de ação nesta sessão.</p>}
    </div>
    {onClose && <button type="button" className="retro-column-advance" disabled={isClosing} onClick={onClose}>
      {isClosing ? "Encerrando…" : "Encerrar sessão"} <span aria-hidden="true">›</span>
    </button>}
  </article>;
}
