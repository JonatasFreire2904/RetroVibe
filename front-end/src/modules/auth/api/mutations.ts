import { useMutation, useQueryClient } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import { clearParticipantMeta } from "@/shared/lib/participantMeta";
import { setParticipantToken } from "@/shared/lib/participantToken";
import { setToken } from "@/shared/lib/tokenStore";
import type { CurrentUser } from "@/shared/types";
import { userKeys } from "@/modules/user/api/queries";

export interface LoginInput {
  username: string;
  password: string;
}

interface LoginResult {
  token: string;
  user: CurrentUser;
}

export function useLoginMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: LoginInput) => httpClient.post<LoginResult>("/auth/login", input),
    onSuccess: (result) => {
      setToken(result.token);
      clearParticipantMeta();
      setParticipantToken(null);
      queryClient.setQueryData(userKeys.me, result.user);
    },
  });
}

export function useLogout() {
  const queryClient = useQueryClient();
  return () => {
    httpClient.post("/auth/logout").catch(() => undefined);
    setToken(null);
    clearParticipantMeta();
    setParticipantToken(null);
    queryClient.clear();
  };
}
