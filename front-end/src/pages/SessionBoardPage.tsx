import type { ReactNode } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useAddCardMutation,
  useAdvancePhaseMutation,
  useCloseSessionMutation,
  useEditCardMutation,
  usePauseSessionMutation,
  useResumeSessionMutation,
  useToggleVoteMutation,
} from "@/modules/sessions/api/mutations";
import { useSessionBoardQuery } from "@/modules/sessions/api/queries";
import { BoardColumn } from "@/modules/sessions/components/BoardColumn";
import { InviteParticipantsBox } from "@/modules/sessions/components/InviteParticipantsBox";
import { PhaseControl } from "@/modules/sessions/components/PhaseControl";
import { formatDate } from "@/shared/lib/format";
import { ApiError } from "@/shared/lib/httpClient";
import { Badge } from "@/shared/ui/Badge";
import { Button } from "@/shared/ui/Button";
import { ArrowRightIcon, CheckSquareIcon, ClockIcon, UsersIcon } from "@/shared/ui/Icons";

export function SessionBoardPage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const { data: board, isLoading, isError, error } = useSessionBoardQuery(sessionId);

  const addCard = useAddCardMutation(sessionId ?? "");
  const editCard = useEditCardMutation(sessionId ?? "");
  const toggleVote = useToggleVoteMutation(sessionId ?? "");
  const closeSession = useCloseSessionMutation(sessionId ?? "");
  const pauseSession = usePauseSessionMutation(sessionId ?? "");
  const resumeSession = useResumeSessionMutation(sessionId ?? "");
  const advancePhase = useAdvancePhaseMutation(sessionId ?? "");

  if (isError) {
    const isForbidden = error instanceof ApiError && error.status === 403;
    return (
      <div className="mx-auto max-w-md rounded-2xl border border-dashed border-slate-200 p-8 text-center">
        <p className="text-sm text-slate-500">
          {isForbidden ? "Você não tem acesso a essa sessão (é de outro squad)." : "Não foi possível carregar essa sessão."}
        </p>
        <Link to="/historico" className="mt-3 inline-block text-sm font-semibold text-violet-600">
          Voltar ao histórico
        </Link>
      </div>
    );
  }

  if (isLoading || !board) {
    return <p className="text-sm text-slate-400">Carregando sessão…</p>;
  }

  const isOpen = board.status !== "COMPLETED";
  const canAddCard = board.status === "ACTIVE" && board.phase === "COLLECTING";
  const canVote = board.status === "ACTIVE" && board.phase === "VOTING";

  async function handleClose() {
    if (!confirm("Encerrar esta sessão? Ela passará a ficar somente para leitura.")) return;
    await closeSession.mutateAsync(undefined);
  }

  return (
    <div className="mx-auto max-w-6xl">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-4">
        <Link to="/historico" className="flex items-center gap-1 text-sm font-medium text-slate-500 hover:text-slate-700">
          <ArrowRightIcon className="rotate-180" width={14} height={14} />
          Voltar ao histórico
        </Link>

        {isOpen && (
          <div className="flex items-center gap-2">
            {board.status === "ACTIVE" ? (
              <Button variant="outline" size="sm" disabled={pauseSession.isPending} onClick={() => pauseSession.mutate()}>
                ⏸️ Pausar
              </Button>
            ) : (
              <Button variant="outline" size="sm" disabled={resumeSession.isPending} onClick={() => resumeSession.mutate()}>
                ▶️ Retomar
              </Button>
            )}
            <Button variant="secondary" size="sm" disabled={closeSession.isPending} onClick={handleClose}>
              {closeSession.isPending ? "Encerrando…" : "Encerrar sessão"}
            </Button>
          </div>
        )}
      </div>

      <div className="mb-6 flex flex-wrap items-center justify-between gap-4 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
        <div className="flex items-center gap-3">
          <span className="text-2xl">{board.theme.emoji}</span>
          <div>
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-lg font-extrabold text-slate-900">{board.title || board.template.label}</h1>
              <Badge variant="cyan">{board.theme.label}</Badge>
              <Badge variant="violet">Squad {board.squad.name}</Badge>
              {board.privacyMode === "ANONYMOUS" && <Badge variant="rose">🕶️ Anônimo</Badge>}
              {board.status === "PAUSED" && <Badge variant="amber">⏸️ Pausada</Badge>}
            </div>
            <p className="mt-1 text-xs text-slate-400">
              {formatDate(board.createdAt)} · {board.status === "COMPLETED" ? "Sessão encerrada" : "Sessão em andamento"}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-6">
          <HeaderStat icon={<UsersIcon width={16} height={16} />} label="participantes" value={board.participantsCount} />
          <HeaderStat icon={<CheckSquareIcon width={16} height={16} />} label="action items" value={board.actionItemsCount} />
          <HeaderStat icon={<ClockIcon width={16} height={16} />} label="duração total" value={`${board.durationMinutes} min`} />
        </div>
      </div>

      {isOpen && (
        <PhaseControl
          phase={board.phase}
          canControl={board.status === "ACTIVE"}
          isAdvancing={advancePhase.isPending}
          onAdvance={() => advancePhase.mutate()}
        />
      )}

      {isOpen ? (
        <InviteParticipantsBox sessionId={board.id} />
      ) : (
        <div className="mb-6 rounded-xl bg-violet-50 px-4 py-3 text-sm text-violet-700">
          📋 Visualização somente leitura — esta sessão foi encerrada em {board.closedAt ? formatDate(board.closedAt) : "—"}.
        </div>
      )}

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {board.columns.map((column) => (
          <BoardColumn
            key={column.id}
            sessionId={board.id}
            column={column}
            canAddCard={canAddCard}
            canVote={canVote}
            canEditOwnCards={isOpen}
            isAddingCard={addCard.isPending}
            onVote={(cardId) => toggleVote.mutate(cardId)}
            onAddCard={(text) => addCard.mutate({ columnId: column.id, text })}
            onEditCard={(cardId, text) => editCard.mutate({ cardId, text })}
          />
        ))}
      </div>

      {closeSession.isSuccess && (
        <div className="mt-6 flex justify-end">
          <Button variant="outline" onClick={() => navigate("/itens-de-acao")}>
            Ir para Itens de Ação
          </Button>
        </div>
      )}
    </div>
  );
}

function HeaderStat({ icon, label, value }: { icon: ReactNode; label: string; value: string | number }) {
  return (
    <div className="flex items-center gap-2 rounded-xl bg-slate-50 px-3 py-2 text-xs">
      <span className="text-violet-500">{icon}</span>
      <div>
        <p className="font-mono font-bold text-slate-800">{value}</p>
        <p className="text-slate-400">{label}</p>
      </div>
    </div>
  );
}
