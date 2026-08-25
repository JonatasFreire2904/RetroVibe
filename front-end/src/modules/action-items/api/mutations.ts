import { useMutation, useQueryClient } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { ActionItem, ActionItemStatus, Comment } from "@/shared/types";
import { actionItemKeys } from "./queries";

export interface CreateActionItemInput {
  sessionId: string;
  description: string;
  assigneeId?: string;
  dueDate?: string;
}

export interface UpdateActionItemInput {
  id: string;
  status?: ActionItemStatus;
  description?: string;
  assigneeId?: string | null;
  dueDate?: string | null;
}

function invalidateActionItems(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: ["action-items"] });
}

export function useCreateActionItemMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateActionItemInput) => httpClient.post<ActionItem>("/action-items", input),
    onSuccess: () => invalidateActionItems(queryClient),
  });
}

export function useUpdateActionItemMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...input }: UpdateActionItemInput) => httpClient.patch<ActionItem>(`/action-items/${id}`, input),
    onSuccess: () => invalidateActionItems(queryClient),
  });
}

export function useAddActionItemCommentMutation(actionItemId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (text: string) => httpClient.post<Comment>(`/action-items/${actionItemId}/comments`, { text }),
    onSuccess: () => {
      invalidateActionItems(queryClient);
      queryClient.invalidateQueries({ queryKey: actionItemKeys.comments(actionItemId) });
    },
  });
}
