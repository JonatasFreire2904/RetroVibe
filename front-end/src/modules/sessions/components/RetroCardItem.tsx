import { useState } from "react";
import { useAddCommentMutation } from "@/modules/sessions/api/mutations";
import { useCardCommentsQuery } from "@/modules/sessions/api/queries";
import type { SessionCard } from "@/shared/types";
import { CommentThread } from "@/shared/ui/CommentThread";
import { MessageIcon, PencilIcon, ThumbsUpIcon } from "@/shared/ui/Icons";

interface RetroCardItemProps {
  sessionId: string;
  card: SessionCard;
  canVote: boolean;
  canEdit: boolean;
  onVote?: () => void;
  onEdit?: (text: string) => void;
}

export function RetroCardItem({ sessionId, card, canVote, canEdit, onVote, onEdit }: RetroCardItemProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(card.text);
  const [showComments, setShowComments] = useState(false);

  const commentsQuery = useCardCommentsQuery(sessionId, card.id, showComments);
  const addComment = useAddCommentMutation(sessionId);

  function handleSave() {
    const text = draft.trim();
    if (!text || text === card.text) {
      setEditing(false);
      return;
    }
    onEdit?.(text);
    setEditing(false);
  }

  return (
    <div className="rounded-xl bg-violet-50/60 p-4">
      {editing ? (
        <div className="space-y-2">
          <textarea
            autoFocus
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            rows={2}
            className="w-full rounded-lg border border-violet-300 bg-white p-2 text-sm outline-none"
          />
          <div className="flex gap-2 text-xs">
            <button onClick={handleSave} className="rounded-full bg-violet-600 px-2.5 py-1 font-semibold text-white">
              Salvar
            </button>
            <button
              onClick={() => {
                setDraft(card.text);
                setEditing(false);
              }}
              className="rounded-full bg-white px-2.5 py-1 font-semibold text-slate-500"
            >
              Cancelar
            </button>
          </div>
        </div>
      ) : (
        <p className="text-sm text-slate-700">{card.text}</p>
      )}

      <div className="mt-3 flex items-center gap-4 text-xs text-slate-500">
        <button
          onClick={onVote}
          disabled={!canVote}
          className={`flex items-center gap-1 rounded-full px-2 py-1 font-mono transition ${
            canVote ? "hover:bg-violet-100 hover:text-violet-600" : ""
          }`}
        >
          <ThumbsUpIcon width={13} height={13} />
          {card.votes} votos
        </button>
        <button
          onClick={() => setShowComments((v) => !v)}
          className={`flex items-center gap-1 rounded-full px-2 py-1 font-mono transition hover:bg-violet-100 hover:text-violet-600 ${
            showComments ? "bg-violet-100 text-violet-600" : ""
          }`}
        >
          <MessageIcon width={13} height={13} />
          {card.commentsCount}
        </button>
        {canEdit && !editing && (
          <button
            onClick={() => setEditing(true)}
            className="ml-auto flex items-center gap-1 text-violet-500 hover:text-violet-700"
            title="Editar sua contribuição"
          >
            <PencilIcon width={13} height={13} />
          </button>
        )}
      </div>

      {showComments && (
        <CommentThread
          comments={commentsQuery.data}
          isLoading={commentsQuery.isLoading}
          onAdd={(text) => addComment.mutate({ cardId: card.id, text })}
          isAdding={addComment.isPending}
        />
      )}
    </div>
  );
}
