import { useEffect, useState, type CSSProperties } from "react";
import { Link } from "react-router-dom";
import type { PrivacyMode, SessionBoard, SessionPhase } from "@/shared/types";
import { useSessionActionItemsQuery, useSessionAssigneesQuery } from "../api/queries";
import { getRetroColumnAppearance, getRetroTheme } from "../retroTheme";
import { ActionBoardColumn } from "./ActionBoardColumn";
import { BoardColumn } from "./BoardColumn";
import { ThemeTrim } from "./ThemeTrim";
import { EditSessionSettingsModal } from "./EditSessionSettingsModal";
import "./retroBoard.css";

interface RetroBoardViewProps {
  board: SessionBoard;
  isFacilitator: boolean;
  participantName?: string;
  isAddingCard: boolean;
  isAdvancing?: boolean;
  isPausing?: boolean;
  isResuming?: boolean;
  isClosing?: boolean;
  isUpdatingSettings?: boolean;
  isRevealingCards?: boolean;
  onAddCard: (columnId: string, text: string) => Promise<void>;
  onEditCard: (cardId: string, text: string) => void;
  onVote: (cardId: string) => void;
  onAdvance?: () => void;
  onNavigateStage?: (phase: SessionPhase, activeColumnIndex: number) => void;
  onUpdateSettings?: (settings: { title: string | null; privacyMode: PrivacyMode; sequentialFlow: boolean; actionCardsEnabled: boolean; cardBlurEnabled: boolean }) => Promise<unknown>;
  onRevealCards?: () => void;
  onPause?: () => void;
  onResume?: () => void;
  onClose?: () => void;
}

function formatTimer(seconds: number): string {
  const minutes = Math.floor(seconds / 60).toString().padStart(2, "0");
  return `${minutes}:${(seconds % 60).toString().padStart(2, "0")}`;
}

export function RetroBoardView({
  board, isFacilitator, participantName, isAddingCard, isAdvancing = false, isPausing = false,
  isResuming = false, isClosing = false, isUpdatingSettings = false, isRevealingCards = false,
  onAddCard, onEditCard, onVote, onAdvance, onNavigateStage, onUpdateSettings, onRevealCards, onPause, onResume, onClose,
}: RetroBoardViewProps) {
  const theme = getRetroTheme(board.theme.key);
  const [inviteOpen, setInviteOpen] = useState(false);
  const [copied, setCopied] = useState(false);
  const [secondsLeft, setSecondsLeft] = useState(25 * 60);
  const [timerRunning, setTimerRunning] = useState(false);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const { data: actionItems = [] } = useSessionActionItemsQuery(board.id, board.actionCardsEnabled);
  const { data: assignees = [] } = useSessionAssigneesQuery(isFacilitator ? board.id : undefined);

  useEffect(() => {
    if (!timerRunning || board.status !== "ACTIVE") return;
    const interval = window.setInterval(() => setSecondsLeft(value => {
      if (value <= 1) {
        setTimerRunning(false);
        return 0;
      }
      return value - 1;
    }), 1000);
    return () => window.clearInterval(interval);
  }, [timerRunning, board.status]);

  const activeIndex = Math.min(board.activeColumnIndex ?? 0, board.columns.length - 1);
  const collecting = board.phase === "COLLECTING";
  const canAdd = board.status === "ACTIVE" && collecting;
  const canVote = board.status === "ACTIVE" && board.phase === "VOTING";
  const advanceIndex = collecting && board.sequentialFlow ? activeIndex : board.columns.length - 1;
  const nextColumn = board.columns[activeIndex + 1];
  const nextLabel = nextColumn ? getRetroColumnAppearance(board, nextColumn, activeIndex + 1).label : "Votação";
  const advanceLabel = collecting
    ? `Avançar para ${nextLabel}`
    : board.phase === "VOTING" ? "Avançar para Discussão" : "Encerrar sessão";
  const inviteLink = `${window.location.origin}/entrar/${board.id}`;

  async function copyInvite() {
    try {
      await navigator.clipboard.writeText(inviteLink);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 2000);
    } catch {
      setCopied(false);
    }
  }

  return (
    <div className={`retro-experience retro-theme--${theme.key}`}>
      <header className="retro-session-header">
        <ThemeTrim decor={theme.decor} />
        <div className="retro-session-header-main">
          <div className="retro-session-identity">
            {isFacilitator ? <Link to="/" className="retro-session-theme-icon" aria-label="Voltar à Home">{theme.icon}</Link> : <span className="retro-session-theme-icon">{theme.icon}</span>}
            <div className="retro-session-name">
              <h1>{board.title || board.template.label}</h1>
              <div className="retro-session-tags">
                <span>{theme.shortLabel ?? board.theme.label}</span>
                <span>Squad {board.squad.name}</span>
                <span className="retro-session-code">#{board.id.slice(0, 7).toUpperCase()}</span>
                {board.privacyMode === "ANONYMOUS" && <span>🕶️ Anônimo</span>}
                {participantName && <span>{participantName}</span>}
              </div>
            </div>
          </div>

          <nav className="retro-stage-nav" aria-label="Etapas da retrospectiva">
            {board.columns.map((column, index) => {
              const visual = getRetroColumnAppearance(board, column, index);
              const current = collecting && (board.sequentialFlow ? index === activeIndex : true);
              const done = !collecting || (board.sequentialFlow && index < activeIndex);
              return (
                <div key={column.id} className="retro-stage-item">
                  <button type="button" disabled={!isFacilitator || board.status !== "ACTIVE" || isAdvancing}
                    onClick={() => onNavigateStage?.("COLLECTING", index)}
                    className={`retro-stage-pill ${current ? "retro-stage-pill--current" : ""} ${done ? "retro-stage-pill--done" : ""}`} title={`Ir para ${visual.label}`}>
                    {done ? "✅" : visual.icon} <span>{visual.label}</span>
                  </button>
                  {index < board.columns.length - 1 && <span className="retro-stage-divider">›</span>}
                </div>
              );
            })}
            <button type="button" disabled={!isFacilitator || board.status !== "ACTIVE"} onClick={() => onNavigateStage?.("VOTING", board.columns.length - 1)}
              className={`retro-stage-phase ${board.phase === "VOTING" ? "retro-stage-pill--current" : ""}`}>🗳️ Votação</button>
            <button type="button" disabled={!isFacilitator || board.status !== "ACTIVE"} onClick={() => onNavigateStage?.("DISCUSSING", board.columns.length - 1)}
              className={`retro-stage-phase ${board.phase === "DISCUSSING" ? "retro-stage-pill--current" : ""}`}>💬 Discussão</button>
          </nav>

          <div className="retro-session-controls">
            {isFacilitator && board.status !== "COMPLETED" && <button type="button" className="retro-header-action" onClick={() => setSettingsOpen(true)}>⚙ <span>Configurações</span></button>}
            {isFacilitator && board.status === "ACTIVE" && board.phase === "COLLECTING" && board.cardBlurEnabled && !board.cardsRevealed &&
              <button type="button" className="retro-header-action" disabled={isRevealingCards} onClick={onRevealCards}>
                👁 <span>{isRevealingCards ? "Revelando…" : "Revelar cards"}</span>
              </button>}
            <div className="retro-timer" aria-label={`Timer ${formatTimer(secondsLeft)}`}>
              <span className="retro-timer-ring" />
              <strong>{formatTimer(secondsLeft)}</strong>
              {isFacilitator && <>
                <button type="button" aria-label={timerRunning ? "Pausar timer" : "Iniciar timer"} onClick={() => setTimerRunning(value => !value)}>{timerRunning ? "Ⅱ" : "▶"}</button>
                <button type="button" aria-label="Reiniciar timer" onClick={() => { setTimerRunning(false); setSecondsLeft(25 * 60); }}>↺</button>
              </>}
            </div>
            {isFacilitator && board.status !== "COMPLETED" && (
              <div className="retro-invite-wrap">
                <button type="button" className="retro-header-action" onClick={() => setInviteOpen(value => !value)}>♧ <span>Convidar</span></button>
                {inviteOpen && <div className="retro-invite-popover">
                  <strong>Convidar participantes</strong>
                  <p>Compartilhe o link da sessão com o time.</p>
                  <input readOnly value={inviteLink} onFocus={event => event.currentTarget.select()} aria-label="Link de convite" />
                  <button type="button" onClick={() => void copyInvite()}>{copied ? "Copiado!" : "Copiar link"}</button>
                </div>}
              </div>
            )}
            {isFacilitator && board.status !== "COMPLETED" && (
              <button type="button" className="retro-header-action retro-header-action--round" disabled={isPausing || isResuming} onClick={board.status === "PAUSED" ? onResume : onPause} title={board.status === "PAUSED" ? "Retomar sessão" : "Pausar sessão"}>
                {board.status === "PAUSED" ? "▶" : "Ⅱ"}
              </button>
            )}
            {isFacilitator && board.status !== "COMPLETED" && <button type="button" className="retro-header-action retro-header-action--close" disabled={isClosing} onClick={onClose}>↪ <span>Encerrar</span></button>}
          </div>
        </div>
        <ThemeTrim decor={theme.decor} />
      </header>

      {board.status === "PAUSED" && <div className="retro-status-banner">⏸️ Sessão pausada pelo facilitador</div>}
      {board.status === "COMPLETED" && <div className="retro-status-banner">✓ Sessão encerrada · visualização somente leitura</div>}

      <main className="retro-board-scroll">
        <div className="retro-board-track" style={{ "--retro-column-count": board.columns.length + (board.actionCardsEnabled ? 1 : 0) } as CSSProperties}>
          {board.columns.map((column, index) => {
            const isFuture = collecting && board.sequentialFlow && index > activeIndex;
            const isCurrent = collecting && (board.sequentialFlow ? index === activeIndex : true);
            const isDone = !collecting || (board.sequentialFlow && index < activeIndex);
            return (
              <BoardColumn
                key={column.id}
                sessionId={board.id}
                column={column}
                appearance={getRetroColumnAppearance(board, column, index)}
                theme={theme}
                isCurrent={isCurrent}
                isFuture={isFuture}
                isDone={isDone}
                canAddCard={canAdd && !isFuture}
                canVote={canVote}
                canEditOwnCards={board.status !== "COMPLETED"}
                cardBlurred={board.cardBlurEnabled && !board.cardsRevealed && collecting}
                isAddingCard={isAddingCard}
                isAdvancing={isAdvancing}
                advanceLabel={isFacilitator && board.status === "ACTIVE" && index === advanceIndex &&
                  !(board.actionCardsEnabled && board.phase === "DISCUSSING") ? advanceLabel : undefined}
                onAdvance={isFacilitator && board.status === "ACTIVE" && index === advanceIndex &&
                  !(board.actionCardsEnabled && board.phase === "DISCUSSING") ? (board.phase === "DISCUSSING" ? onClose : onAdvance) : undefined}
                onVote={onVote}
                onAddCard={(text) => onAddCard(column.id, text)}
                onEditCard={onEditCard}
              />
            );
          })}
          {board.actionCardsEnabled && <ActionBoardColumn sessionId={board.id} theme={theme}
            palette={theme.palettes[board.columns.length % theme.palettes.length]}
            items={actionItems} assignees={assignees} isFacilitator={isFacilitator}
            isActive={board.status === "ACTIVE"} isClosing={isClosing}
            onClose={isFacilitator && board.status === "ACTIVE" && board.phase === "DISCUSSING" ? onClose : undefined} />}
        </div>
      </main>
      {isFacilitator && onUpdateSettings && <EditSessionSettingsModal board={board} open={settingsOpen} saving={isUpdatingSettings}
        onClose={() => setSettingsOpen(false)} onSave={onUpdateSettings} />}
    </div>
  );
}
