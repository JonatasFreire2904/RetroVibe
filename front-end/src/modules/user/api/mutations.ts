import { useMutation, useQueryClient } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { CurrentUser } from "@/shared/types";
import { userKeys } from "./queries";

export interface UpdateProfileInput {
  name: string;
  role: string;
  avatarColor: string;
}

export function useUpdateProfileMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: UpdateProfileInput) => httpClient.patch<CurrentUser>("/me", input),
    onSuccess: (user) => {
      queryClient.setQueryData(userKeys.me, user);
    },
  });
}
