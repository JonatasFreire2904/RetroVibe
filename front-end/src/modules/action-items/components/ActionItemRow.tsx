import { useState } from "react";
import type { ActionItem, ActionItemStatus } from "@/shared/types";
import { formatDate } from "@/shared/lib/format";
import { Avatar } from "@/shared/ui/Avatar";
import { Button } from "@/shared/ui/Button";
import { CommentThread } from "@/shared/ui/CommentThread";
import { CalendarIcon, MessageIcon } from "@/shared/ui/Icons";
import { useAddActionItemCommentMutation, useUpdateActionItemMutation } from "../api/mutations";
import { useActionItemCommentsQuery } from "../api/queries";

const STATUS_OPTIONS: { value: ActionItemStatus; label: string; className: string }[] = [
  { value: "PLANNED", label: "Planejado", className: "bg-sky-100 text-sky-700" },
  { value: "BACKLOG", label: "Backlog", className: "bg-slate-100 text-slate-600" },
  { value: "IN_PROGRESS", label: "Em andamento", className: "bg-amber-100 text-amber-700" },
  { value: "DONE", label: "Concluído", className: "bg-emerald-100 text-emerald-700" },
  { value: "DISCARDED", label: "Descartado", className: "bg-rose-100 text-rose-700" },
];

const BORDER_BY_STATUS: Record<ActionItemStatus, string> = {
  PLANNED: "border-l-sky-400",
  BACKLOG: "border-l-slate-300",
  IN_PROGRESS: "border-l-amber-400",
  DONE: "border-l-emerald-400",
  DISCARDED: "border-l-rose-400",
};

export function ActionItemRow({ item, assignees = [] }: { item: ActionItem; assignees?: { id: string; name: string; avatarColor: string }[] }) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(item.description);
  const [showComments, setShowComments] = useState(false);
  const updateItem = useUpdateActionItemMutation();
  const commentsQuery = useActionItemCommentsQuery(item.id, showComments);
  const addComment = useAddActionItemCommentMutation(item.id);

  const statusMeta = STATUS_OPTIONS.find((s) => s.value === item.status)!;

  function handleStatusChange(status: ActionItemStatus) {
    updateItem.mutate({ id: item.id, status });
  }

  function handleSave() {
    const description = draft.trim();
    if (!description || description === item.description) {
      setEditing(false);
      return;
    }
    updateItem.mutate({ id: item.id, description }, { onSuccess: () => setEditing(false) });
  }

  return (
    <div className={`rounded-xl border-l-4 bg-white p-4 shadow-sm ${BORDER_BY_STATUS[item.status]}`}>
      <div className="flex flex-wrap items-center gap-3">
        <select
          value={item.status}
          onChange={(e) => handleStatusChange(e.target.value as ActionItemStatus)}
          className={`rounded-full border-0 px-3 py-1 text-xs font-semibold outline-none ${statusMeta.className}`}
        >
          {STATUS_OPTIONS.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>

        {editing ? (
          <div className="flex min-w-[240px] flex-1 items-center gap-2">
            <input
              autoFocus
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSave()}
              className="flex-1 rounded-lg border border-violet-300 px-3 py-1.5 text-sm outline-none"
            />
            <Button size="sm" onClick={handleSave} disabled={updateItem.isPending}>
              Salvar
            </Button>
            <Button size="sm" variant="outline" onClick={() => { setDraft(item.description); setEditing(false); }}>
              Cancelar
            </Button>
          </div>
        ) : (
          <button
            onClick={() => setEditing(true)}
            className="min-w-[200px] flex-1 text-left text-sm text-slate-700 hover:text-slate-900"
          >
            {item.description}
          </button>
        )}

        <div className="ml-auto flex items-center gap-4">
          {item.assignee && <Avatar name={item.assignee.name} color={item.assignee.avatarColor} size="sm" />}
          <select aria-label={`Responsável por ${item.description}`} title="Editar responsável"
            value={item.assignee?.id ?? ""} onChange={event => updateItem.mutate({ id: item.id, assigneeId: event.target.value || null })}
            disabled={updateItem.isPending}
            className="max-w-[145px] rounded-lg border border-slate-200 bg-white px-2 py-1 text-xs text-slate-600 outline-none focus:border-violet-400">
            <option value="">Sem responsável</option>
            {assignees.map(user => <option key={user.id} value={user.id}>{user.name}</option>)}
            {item.assignee && !assignees.some(user => user.id === item.assignee?.id) &&
              <option value={item.assignee.id}>{item.assignee.name}</option>}
          </select>
          {item.dueDate && (
            <span className="flex items-center gap-1 rounded-full bg-slate-50 px-2.5 py-1 font-mono text-xs text-slate-500">
              <CalendarIcon width={12} height={12} />
              {formatDate(item.dueDate)}
            </span>
          )}
          <button
            onClick={() => setShowComments((v) => !v)}
            aria-label={`Comentários: ${item.commentsCount}`}
            className={`flex items-center gap-1 rounded-full px-2 py-1 text-xs transition hover:bg-violet-100 hover:text-violet-600 ${
              showComments ? "bg-violet-100 text-violet-600" : "text-slate-400"
            }`}
          >
            <MessageIcon width={13} height={13} />
            {item.commentsCount}
          </button>
        </div>
      </div>

      {showComments && (
        <CommentThread
          comments={commentsQuery.data}
          isLoading={commentsQuery.isLoading}
          onAdd={(text) => addComment.mutate(text)}
          isAdding={addComment.isPending}
        />
      )}
    </div>
  );
}
