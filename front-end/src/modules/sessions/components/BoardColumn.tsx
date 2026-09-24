import { useState, type CSSProperties } from "react";
import type { SessionColumn } from "@/shared/types";
import type { RetroColumnAppearance, RetroTheme } from "../retroTheme";
import { RetroCardItem } from "./RetroCardItem";
import { ThemeTrim } from "./ThemeTrim";

interface BoardColumnProps {
  sessionId: string;
  column: SessionColumn;
  appearance: RetroColumnAppearance;
  theme: RetroTheme;
  isCurrent: boolean;
  isFuture: boolean;
  isDone: boolean;
  canAddCard: boolean;
  canVote: boolean;
  canEditOwnCards: boolean;
  cardBlurred: boolean;
  isAddingCard: boolean;
  isAdvancing: boolean;
  advanceLabel?: string;
  onAdvance?: () => void;
  onVote: (cardId: string) => void;
  onAddCard: (text: string) => Promise<void>;
  onEditCard: (cardId: string, text: string) => void;
}

export function BoardColumn({
  sessionId, column, appearance, theme, isCurrent, isFuture, isDone, canAddCard, canVote,
  canEditOwnCards, cardBlurred, isAddingCard, isAdvancing, advanceLabel, onAdvance, onVote, onAddCard, onEditCard,
}: BoardColumnProps) {
  const [draft, setDraft] = useState("");
  const [composing, setComposing] = useState(false);
  const [submitError, setSubmitError] = useState(false);

  async function submit() {
    const text = draft.trim();
    if (!text || isAddingCard) return;
    setSubmitError(false);
    try {
      await onAddCard(text);
      setDraft("");
      setComposing(false);
    } catch {
      setSubmitError(true);
    }
  }

  const style = {
    "--retro-column-bg": appearance.background,
    "--retro-column-border": appearance.border,
    "--retro-column-accent": appearance.accent,
    "--retro-card-bg": appearance.card,
  } as CSSProperties;

  return (
    <article className={`retro-column ${isCurrent ? "retro-column--current" : ""} ${isFuture ? "retro-column--future" : ""}`} style={style}>
      <ThemeTrim decor={theme.decor} small />
      <header className="retro-column-heading">
        <div className="retro-column-kicker">
          <span className="retro-column-icon">{appearance.icon}</span>
          {isCurrent && <span className="retro-column-status">ATIVO</span>}
          {isDone && <span className="retro-column-status retro-column-status--done">✓ FEITO</span>}
          <span className="retro-column-count">{column.cards.length}</span>
        </div>
        <h2>{appearance.label}</h2>
        <p>{appearance.prompt}</p>
        <div className="retro-column-rule" />
      </header>

      <div className="retro-column-content">
        {column.cards.map((card) => (
          <RetroCardItem
            key={card.id}
            sessionId={sessionId}
            card={card}
            theme={theme}
            blurred={isFuture || (cardBlurred && !card.isMine)}
            blurMessage={isFuture ? "Aguardando etapa anterior" : "Aguardando o facilitador revelar os cards"}
            canVote={canVote}
            canEdit={canEditOwnCards && card.isMine}
            onVote={() => onVote(card.id)}
            onEdit={(text) => onEditCard(card.id, text)}
          />
        ))}

        {isFuture && <p className="retro-column-wait">🔒 Aguardando etapa anterior</p>}

        {canAddCard && (composing ? (
          <div className="retro-card-composer">
            <textarea
              autoFocus
              value={draft}
              onChange={(event) => setDraft(event.target.value)}
              onKeyDown={(event) => {
                if ((event.ctrlKey || event.metaKey) && event.key === "Enter") void submit();
                if (event.key === "Escape") setComposing(false);
              }}
              placeholder={theme.key === "natal" ? "O que você quer contar ao Papai Noel?" : appearance.prompt}
              rows={3}
            />
            {submitError && <p className="retro-composer-error">Não foi possível adicionar. Tente novamente.</p>}
            <div className="retro-composer-actions">
              <button type="button" onClick={() => void submit()} disabled={!draft.trim() || isAddingCard}>
                {appearance.icon} Adicionar
              </button>
              <button type="button" className="retro-composer-cancel" onClick={() => setComposing(false)}>Cancelar</button>
            </div>
          </div>
        ) : (
          <button type="button" className="retro-add-card" onClick={() => setComposing(true)}>
            <span aria-hidden="true">＋</span> {theme.addLabel}
          </button>
        ))}
      </div>

      {onAdvance && advanceLabel && (
        <button type="button" className="retro-column-advance" disabled={isAdvancing} onClick={onAdvance}>
          {isAdvancing ? "Avançando…" : advanceLabel} <span aria-hidden="true">›</span>
        </button>
      )}
    </article>
  );
}
