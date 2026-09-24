import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import { getParticipantToken } from "@/shared/lib/participantToken";
import type { ActionItem, Comment, HomeData, SessionBoard, SessionSummary } from "@/shared/types";

export const sessionKeys = {
  home: ["sessions", "home"] as const,
  list: (filters: SessionListFilters) => ["sessions", "list", filters] as const,
  board: (id: string) => ["sessions", "board", id] as const,
  cardComments: (sessionId: string, cardId: string) => ["sessions", "board", sessionId, "cards", cardId, "comments"] as const,
};

export function useHomeDataQuery() {
  return useQuery({
    queryKey: sessionKeys.home,
    queryFn: () => httpClient.get<HomeData>("/home"),
  });
}

export interface SessionListFilters {
  squadId?: string;
  templateId?: string;
  themeId?: string;
  search?: string;
}

export function useSessionsQuery(filters: SessionListFilters) {
  return useQuery({
    queryKey: sessionKeys.list(filters),
    queryFn: () =>
      httpClient.get<SessionSummary[]>("/sessions", {
        squadId: filters.squadId,
        templateId: filters.templateId,
        themeId: filters.themeId,
        search: filters.search,
      }),
  });
}

export function useSessionBoardQuery(sessionId: string | undefined, options?: { refetchInterval?: number }) {
  const viewerKey = window.location.pathname.startsWith("/participar/") ? getParticipantToken() : "staff";
  return useQuery({
    queryKey: [...sessionKeys.board(sessionId ?? ""), viewerKey],
    queryFn: () => httpClient.get<SessionBoard>(`/sessions/${sessionId}`),
    enabled: Boolean(sessionId),
    refetchInterval: options?.refetchInterval,
  });
}

export function useCardCommentsQuery(sessionId: string, cardId: string, enabled: boolean) {
  return useQuery({
    queryKey: sessionKeys.cardComments(sessionId, cardId),
    queryFn: () => httpClient.get<Comment[]>(`/sessions/${sessionId}/cards/${cardId}/comments`),
    enabled,
  });
}

export function useSessionAssigneesQuery(sessionId: string | undefined) {
  return useQuery({
    queryKey: ["sessions", "assignees", sessionId],
    queryFn: () => httpClient.get<{ id: string; name: string; avatarColor: string }[]>(`/sessions/${sessionId}/assignees`),
    enabled: Boolean(sessionId),
  });
}

export function useSessionActionItemsQuery(sessionId: string, enabled: boolean) {
  return useQuery({
    queryKey: ["action-items", "session", sessionId],
    queryFn: () => httpClient.get<ActionItem[]>(`/sessions/${sessionId}/action-items`),
    enabled,
    refetchInterval: enabled ? 3000 : false,
  });
}
