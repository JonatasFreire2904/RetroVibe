import { Navigate, useParams } from "react-router-dom";
import { useAddCardMutation, useEditCardMutation, useToggleVoteMutation } from "@/modules/sessions/api/mutations";
import { useSessionBoardQuery } from "@/modules/sessions/api/queries";
import { BoardColumn } from "@/modules/sessions/components/BoardColumn";
import { PhaseControl } from "@/modules/sessions/components/PhaseControl";
import { getParticipantMeta } from "@/shared/lib/participantMeta";
import { Avatar } from "@/shared/ui/Avatar";
import { Badge } from "@/shared/ui/Badge";

export function ParticipantSessionPage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const meta = getParticipantMeta();

  const { data: board, isLoading } = useSessionBoardQuery(sessionId, { refetchInterval: 4000 });
  const addCard = useAddCardMutation(sessionId ?? "");
  const editCard = useEditCardMutation(sessionId ?? "");
  const toggleVote = useToggleVoteMutation(sessionId ?? "");

  if (!meta || meta.sessionId !== sessionId) {
    return <Navigate to={`/entrar/${sessionId ?? ""}`} replace />;
  }

  const isOpen = board ? board.status !== "COMPLETED" : false;
  const canAddCard = board?.status === "ACTIVE" && board.phase === "COLLECTING";
  const canVote = board?.status === "ACTIVE" && board.phase === "VOTING";

  return (
    <div className="min-h-screen bg-violet-50/40">
      <header className="flex items-center justify-between border-b border-slate-100 bg-white px-6 py-4">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-xl bg-brand-gradient text-base">🔁</div>
          <span className="font-display text-sm font-extrabold text-slate-900">RetroVibe</span>
        </div>
        <div className="flex items-center gap-2">
          <Avatar name={meta.name} color="#7C3AED" size="sm" />
          <span className="text-sm text-slate-600">{meta.name}</span>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8">
        {isLoading || !board ? (
          <p className="text-sm text-slate-400">Carregando sessão…</p>
        ) : (
          <>
            <div className="mb-6 flex flex-wrap items-center gap-3 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
              <span className="text-2xl">{board.theme.emoji}</span>
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-lg font-extrabold text-slate-900">{board.title || board.template.label}</h1>
                  <Badge variant="cyan">{board.theme.label}</Badge>
                  {board.privacyMode === "ANONYMOUS" && <Badge variant="rose">🕶️ Anônimo</Badge>}
                  {board.status === "PAUSED" && <Badge variant="amber">⏸️ Pausada pelo facilitador</Badge>}
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {board.status === "COMPLETED" ? "Esta sessão já foi encerrada." : "Sessão em andamento — contribua abaixo!"}
                </p>
              </div>
            </div>

            {isOpen && <PhaseControl phase={board.phase} canControl={false} isAdvancing={false} onAdvance={() => undefined} />}

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
              {board.columns.map((column) => (
                <BoardColumn
                  key={column.id}
                  sessionId={board.id}
                  column={column}
                  canAddCard={Boolean(canAddCard)}
                  canVote={Boolean(canVote)}
                  canEditOwnCards={isOpen}
                  isAddingCard={addCard.isPending}
                  onVote={(cardId) => toggleVote.mutate(cardId)}
                  onAddCard={(text) => addCard.mutate({ columnId: column.id, text })}
                  onEditCard={(cardId, text) => editCard.mutate({ cardId, text })}
                />
              ))}
            </div>
          </>
        )}
      </main>
    </div>
  );
}
