import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  useAddCardMutation, useAdvancePhaseMutation, useCloseSessionMutation, useEditCardMutation,
  usePauseSessionMutation, useResumeSessionMutation, useToggleVoteMutation,
  useNavigateStageMutation, useUpdateSessionSettingsMutation,
  useRevealSessionCardsMutation,
} from "@/modules/sessions/api/mutations";
import { useSessionBoardQuery } from "@/modules/sessions/api/queries";
import { RetroBoardView } from "@/modules/sessions/components/RetroBoardView";
import { ApiError } from "@/shared/lib/httpClient";
import { ConfirmDialog } from "@/shared/ui/ConfirmDialog";

export function SessionBoardPage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const { data: board, isLoading, isError, error } = useSessionBoardQuery(sessionId, { refetchInterval: 3000 });
  const addCard = useAddCardMutation(sessionId ?? "");
  const editCard = useEditCardMutation(sessionId ?? "");
  const toggleVote = useToggleVoteMutation(sessionId ?? "");
  const closeSession = useCloseSessionMutation(sessionId ?? "");
  const pauseSession = usePauseSessionMutation(sessionId ?? "");
  const resumeSession = useResumeSessionMutation(sessionId ?? "");
  const advancePhase = useAdvancePhaseMutation(sessionId ?? "");
  const navigateStage = useNavigateStageMutation(sessionId ?? "");
  const updateSettings = useUpdateSessionSettingsMutation(sessionId ?? "");
  const revealCards = useRevealSessionCardsMutation(sessionId ?? "");
  const [confirmingClose, setConfirmingClose] = useState(false);

  if (isError) {
    const message = error instanceof ApiError && error.status === 403
      ? "Você não tem acesso a essa sessão (é de outro squad)."
      : "Não foi possível carregar essa sessão.";
    return <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-violet-50 text-center">
      <p className="text-sm text-slate-600">{message}</p>
      <Link to="/historico" className="text-sm font-bold text-violet-600">Voltar ao histórico</Link>
    </div>;
  }

  if (isLoading || !board) {
    return <div className="flex min-h-screen items-center justify-center bg-violet-50 text-sm text-slate-500">Carregando sessão…</div>;
  }

  function handleClose() {
    closeSession.mutate(undefined, {
      onSuccess: () => navigate(board?.surveyEnabled ? `/pesquisa/${sessionId}` : "/", { replace: true }),
    });
  }

  return (
    <>
      <ConfirmDialog
        open={confirmingClose}
        title="Encerrar sessão"
        message="Depois de encerrada, a sessão fica somente para leitura e ninguém poderá adicionar cards ou votos."
        confirmLabel="Encerrar sessão"
        pendingLabel="Encerrando…"
        tone="danger"
        isPending={closeSession.isPending}
        error={closeSession.isError ? "Não foi possível encerrar a sessão. Tente novamente." : null}
        onConfirm={handleClose}
        onCancel={() => { closeSession.reset(); setConfirmingClose(false); }}
      />
      <RetroBoardView
        board={board}
        isFacilitator
        isAddingCard={addCard.isPending}
        isAdvancing={advancePhase.isPending}
        isPausing={pauseSession.isPending}
        isResuming={resumeSession.isPending}
        isClosing={closeSession.isPending}
        isUpdatingSettings={updateSettings.isPending}
        isRevealingCards={revealCards.isPending}
        onAddCard={async (columnId, text) => { await addCard.mutateAsync({ columnId, text }); }}
        onEditCard={(cardId, text) => editCard.mutate({ cardId, text })}
        onVote={(cardId) => toggleVote.mutate(cardId)}
        onAdvance={() => advancePhase.mutate()}
        onNavigateStage={(phase, activeColumnIndex) => navigateStage.mutate({ phase, activeColumnIndex })}
        onUpdateSettings={(settings) => updateSettings.mutateAsync(settings)}
        onRevealCards={() => revealCards.mutate()}
        onPause={() => pauseSession.mutate()}
        onResume={() => resumeSession.mutate()}
        onClose={() => setConfirmingClose(true)}
      />
    </>
  );
}
