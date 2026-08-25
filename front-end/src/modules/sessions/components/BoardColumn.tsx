import { useState } from "react";
import type { SessionColumn } from "@/shared/types";
import { PlusIcon } from "@/shared/ui/Icons";
import { RetroCardItem } from "./RetroCardItem";

interface BoardColumnProps {
  sessionId: string;
  column: SessionColumn;
  canAddCard: boolean;
  canVote: boolean;
  canEditOwnCards: boolean;
  onVote: (cardId: string) => void;
  onAddCard: (text: string) => void;
  onEditCard: (cardId: string, text: string) => void;
  isAddingCard: boolean;
}

export function BoardColumn({
  sessionId,
  column,
  canAddCard,
  canVote,
  canEditOwnCards,
  onVote,
  onAddCard,
  onEditCard,
  isAddingCard,
}: BoardColumnProps) {
  const [draft, setDraft] = useState("");

  function submit() {
    const text = draft.trim();
    if (!text) return;
    onAddCard(text);
    setDraft("");
  }

  return (
    <div className="flex flex-col rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
      <div className="mb-3 flex items-center justify-between border-b border-slate-100 pb-3">
        <span className="flex items-center gap-2 text-sm font-bold text-slate-800">
          <span>{column.icon}</span>
          {column.label}
        </span>
        <span className="font-mono text-xs text-violet-500">{column.cards.length}</span>
      </div>

      <div className="flex-1 space-y-3">
        {column.cards.map((card) => (
          <RetroCardItem
            key={card.id}
            sessionId={sessionId}
            card={card}
            canVote={canVote}
            canEdit={canEditOwnCards && card.isMine}
            onVote={() => onVote(card.id)}
            onEdit={(text) => onEditCard(card.id, text)}
          />
        ))}
        {column.cards.length === 0 && <p className="py-6 text-center text-xs text-slate-300">Nenhum item ainda</p>}
      </div>

      {canAddCard && (
        <div className="mt-3 flex items-center gap-2 border-t border-slate-100 pt-3">
          <input
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && submit()}
            placeholder="Adicionar item..."
            className="flex-1 rounded-lg border border-slate-200 px-3 py-2 text-xs outline-none placeholder:text-slate-400 focus:border-violet-400"
          />
          <button
            onClick={submit}
            disabled={isAddingCard || !draft.trim()}
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-violet-100 text-violet-600 transition hover:bg-violet-200 disabled:opacity-40"
          >
            <PlusIcon width={14} height={14} />
          </button>
        </div>
      )}
    </div>
  );
}
