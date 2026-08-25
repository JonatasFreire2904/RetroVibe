import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { Comment, HomeData, SessionBoard, SessionSummary } from "@/shared/types";

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

const BOARD_CACHE_PREFIX = "retrovibe_board_cache_";

function cacheBoard(sessionId: string, board: SessionBoard): void {
  try {
    localStorage.setItem(BOARD_CACHE_PREFIX + sessionId, JSON.stringify(board));
  } catch {
  }
}

function readCachedBoard(sessionId: string): SessionBoard | undefined {
  try {
    const raw = localStorage.getItem(BOARD_CACHE_PREFIX + sessionId);
    return raw ? (JSON.parse(raw) as SessionBoard) : undefined;
  } catch {
    return undefined;
  }
}

export function useSessionBoardQuery(sessionId: string | undefined, options?: { refetchInterval?: number }) {
  return useQuery({
    queryKey: sessionKeys.board(sessionId ?? ""),
    queryFn: async () => {
      const board = await httpClient.get<SessionBoard>(`/sessions/${sessionId}`);
      if (sessionId) cacheBoard(sessionId, board);
      return board;
    },
    enabled: Boolean(sessionId),
    refetchInterval: options?.refetchInterval,
    initialData: sessionId ? readCachedBoard(sessionId) : undefined,
    initialDataUpdatedAt: 0,
  });
}

export function useCardCommentsQuery(sessionId: string, cardId: string, enabled: boolean) {
  return useQuery({
    queryKey: sessionKeys.cardComments(sessionId, cardId),
    queryFn: () => httpClient.get<Comment[]>(`/sessions/${sessionId}/cards/${cardId}/comments`),
    enabled,
  });
}
