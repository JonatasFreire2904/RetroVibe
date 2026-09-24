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

  async function handleClose() {
    if (!window.confirm("Encerrar esta sessão? Ela ficará somente para leitura.")) return;
    await closeSession.mutateAsync(undefined);
    navigate("/", { replace: true });
  }

  return (
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
      onClose={() => void handleClose()}
    />
  );
}
