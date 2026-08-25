import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { ActionItem, ActionItemSessionSummary, ActionItemStatus, Comment } from "@/shared/types";

export interface ActionItemFilters {
  sessionId?: string;
  squadId?: string;
  templateId?: string;
  themeId?: string;
  status?: ActionItemStatus;
}

export const actionItemKeys = {
  list: (filters: ActionItemFilters) => ["action-items", "list", filters] as const,
  sessionsSummary: (filters: Omit<ActionItemFilters, "sessionId" | "status">) =>
    ["action-items", "sessions-summary", filters] as const,
  comments: (actionItemId: string) => ["action-items", actionItemId, "comments"] as const,
};

export function useActionItemsQuery(filters: ActionItemFilters) {
  return useQuery({
    queryKey: actionItemKeys.list(filters),
    queryFn: () =>
      httpClient.get<ActionItem[]>("/action-items", {
        sessionId: filters.sessionId,
        squadId: filters.squadId,
        templateId: filters.templateId,
        themeId: filters.themeId,
        status: filters.status,
      }),
    enabled: Boolean(filters.sessionId),
  });
}

export function useActionItemSessionsSummaryQuery(filters: Omit<ActionItemFilters, "sessionId" | "status">) {
  return useQuery({
    queryKey: actionItemKeys.sessionsSummary(filters),
    queryFn: () =>
      httpClient.get<ActionItemSessionSummary[]>("/action-items/sessions-summary", {
        squadId: filters.squadId,
        templateId: filters.templateId,
        themeId: filters.themeId,
      }),
  });
}

export function useActionItemCommentsQuery(actionItemId: string, enabled: boolean) {
  return useQuery({
    queryKey: actionItemKeys.comments(actionItemId),
    queryFn: () => httpClient.get<Comment[]>(`/action-items/${actionItemId}/comments`),
    enabled,
  });
}
