import { Navigate, useParams } from "react-router-dom";
import { useAddCardMutation, useEditCardMutation, useToggleVoteMutation } from "@/modules/sessions/api/mutations";
import { useSessionBoardQuery } from "@/modules/sessions/api/queries";
import { RetroBoardView } from "@/modules/sessions/components/RetroBoardView";
import { getParticipantMeta } from "@/shared/lib/participantMeta";
import { getParticipantToken } from "@/shared/lib/participantToken";

export function ParticipantSessionPage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const meta = getParticipantMeta();
  const { data: board, isLoading, isError } = useSessionBoardQuery(sessionId, { refetchInterval: 3000 });
  const addCard = useAddCardMutation(sessionId ?? "");
  const editCard = useEditCardMutation(sessionId ?? "");
  const toggleVote = useToggleVoteMutation(sessionId ?? "");

  if (!meta || meta.sessionId !== sessionId || !getParticipantToken()) {
    return <Navigate to={`/entrar/${sessionId ?? ""}`} replace />;
  }

  if (isLoading || !board) {
    return <div className="flex min-h-screen items-center justify-center bg-violet-50 text-sm text-slate-500">
      {isError ? "Não foi possível carregar a sessão." : "Carregando sessão…"}
    </div>;
  }

  return (
    <RetroBoardView
      board={board}
      isFacilitator={false}
      participantName={meta.name}
      isAddingCard={addCard.isPending}
      onAddCard={async (columnId, text) => { await addCard.mutateAsync({ columnId, text }); }}
      onEditCard={(cardId, text) => editCard.mutate({ cardId, text })}
      onVote={(cardId) => toggleVote.mutate(cardId)}
    />
  );
}
