import { useState } from "react";
import { useAddCommentMutation } from "@/modules/sessions/api/mutations";
import { useCardCommentsQuery } from "@/modules/sessions/api/queries";
import type { SessionCard } from "@/shared/types";
import { CommentThread } from "@/shared/ui/CommentThread";
import { MessageIcon, PencilIcon, ThumbsUpIcon } from "@/shared/ui/Icons";
import type { RetroTheme } from "../retroTheme";

interface RetroCardItemProps {
  sessionId: string;
  card: SessionCard;
  theme: RetroTheme;
  blurred: boolean;
  canVote: boolean;
  canEdit: boolean;
  onVote: () => void;
  onEdit: (text: string) => void;
}

export function RetroCardItem({ sessionId, card, theme, blurred, canVote, canEdit, onVote, onEdit }: RetroCardItemProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(card.text);
  const [showComments, setShowComments] = useState(false);
  const commentsQuery = useCardCommentsQuery(sessionId, card.id, showComments);
  const addComment = useAddCommentMutation(sessionId);

  function save() {
    const text = draft.trim();
    if (text && text !== card.text) onEdit(text);
    setEditing(false);
  }

  const author = card.authorId === null ? "Anônimo" : card.isMine ? "Você" : "Participante";

  return (
    <div className="retro-card">
      <div className={blurred ? "retro-card-obscured" : ""}>
        {editing ? (
          <div className="retro-card-edit">
            <textarea autoFocus value={draft} onChange={(event) => setDraft(event.target.value)} rows={3} />
            <div>
              <button type="button" onClick={save}>Salvar</button>
              <button type="button" onClick={() => { setDraft(card.text); setEditing(false); }}>Cancelar</button>
            </div>
          </div>
        ) : (
          <p className="retro-card-text"><span aria-hidden="true">{theme.cardIcon}</span> {card.text}</p>
        )}
        <div className="retro-card-meta">
          <span className="retro-card-author"><span className="retro-avatar">{author.charAt(0)}</span>{author}</span>
          <div className="retro-card-actions">
            <button type="button" aria-label={`Comentários: ${card.commentsCount}`} onClick={() => setShowComments(value => !value)}>
              <MessageIcon width={13} height={13} /> {card.commentsCount}
            </button>
            <button type="button" disabled={!canVote} aria-label={`Votos: ${card.votes}`} onClick={onVote}>
              <ThumbsUpIcon width={13} height={13} /> {card.votes}
            </button>
            {canEdit && !editing && <button type="button" aria-label="Editar card" onClick={() => setEditing(true)}><PencilIcon width={13} height={13} /></button>}
          </div>
        </div>
      </div>
      {blurred && <div className="retro-card-lock">🔒 Oculto até a votação</div>}
      {showComments && !blurred && (
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
