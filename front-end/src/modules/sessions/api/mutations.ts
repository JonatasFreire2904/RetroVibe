import { useMutation, useQueryClient } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import { setParticipantMeta } from "@/shared/lib/participantMeta";
import { setParticipantToken } from "@/shared/lib/participantToken";
import type { PrivacyMode, SessionBoard, SessionCard, SessionPhase } from "@/shared/types";
import { sessionKeys } from "./queries";

interface JoinSessionResult {
  token: string;
  participant: { id: string; name: string; sessionId: string };
}

export function useJoinSessionMutation(sessionId: string) {
  return useMutation({
    mutationFn: (displayName: string) =>
      httpClient.post<JoinSessionResult>(`/sessions/${sessionId}/join`, { displayName }),
    onSuccess: (result) => {
      setParticipantToken(result.token);
      setParticipantMeta(result.participant);
    },
  });
}

export interface CreateSessionInput {
  title?: string;
  templateId: string;
  themeId?: string;
  squadName: string;
  privacyMode?: PrivacyMode;
  sequentialFlow?: boolean;
  actionCardsEnabled?: boolean;
  cardBlurEnabled?: boolean;
  isTest?: boolean;
}

export function useCreateSessionMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateSessionInput) => httpClient.post<SessionBoard>("/sessions", input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.home });
      queryClient.invalidateQueries({ queryKey: ["sessions", "list"] });
    },
  });
}

function useSessionMutation<TInput = void>(sessionId: string, path: string, method: "post" | "patch" = "patch") {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: TInput) =>
      method === "patch"
        ? httpClient.patch<SessionBoard>(`/sessions/${sessionId}${path}`, input)
        : httpClient.post<SessionBoard>(`/sessions/${sessionId}${path}`, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
    },
  });
}

export function useAddCardMutation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: { columnId: string; text: string }) =>
      httpClient.post<SessionCard>(`/sessions/${sessionId}/cards`, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
    },
  });
}

export function useEditCardMutation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ cardId, text }: { cardId: string; text: string }) =>
      httpClient.patch<SessionBoard>(`/sessions/${sessionId}/cards/${cardId}`, { text }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
    },
  });
}

export function useToggleVoteMutation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (cardId: string) =>
      httpClient.post<{ voted: boolean; votes: number }>(`/sessions/${sessionId}/votes`, { cardId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
    },
  });
}

export function useAddCommentMutation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: { cardId: string; text: string }) =>
      httpClient.post<{ commentsCount: number }>(`/sessions/${sessionId}/comments`, input),
    onSuccess: (_result, input) => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
      queryClient.invalidateQueries({ queryKey: sessionKeys.cardComments(sessionId, input.cardId) });
    },
  });
}

export function useCloseSessionMutation(sessionId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (feedbackScore?: number) =>
      httpClient.patch<SessionBoard>(`/sessions/${sessionId}/close`, { feedbackScore }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: sessionKeys.board(sessionId) });
      queryClient.invalidateQueries({ queryKey: sessionKeys.home });
      queryClient.invalidateQueries({ queryKey: ["sessions", "list"] });
    },
  });
}

export function usePauseSessionMutation(sessionId: string) {
  return useSessionMutation(sessionId, "/pause");
}

export function useResumeSessionMutation(sessionId: string) {
  return useSessionMutation(sessionId, "/resume");
}

export function useAdvancePhaseMutation(sessionId: string) {
  return useSessionMutation(sessionId, "/phase");
}

export function useNavigateStageMutation(sessionId: string) {
  return useSessionMutation<{ phase: SessionPhase; activeColumnIndex: number }>(sessionId, "/stage");
}

export function useRevealSessionCardsMutation(sessionId: string) {
  return useSessionMutation(sessionId, "/reveal-cards");
}

export function useUpdateSessionSettingsMutation(sessionId: string) {
  return useSessionMutation<{ title: string | null; privacyMode: PrivacyMode; sequentialFlow: boolean; actionCardsEnabled: boolean; cardBlurEnabled: boolean }>(sessionId, "/settings");
}
