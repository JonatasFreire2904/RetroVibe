import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { CurrentUser } from "@/shared/types";

export const userKeys = {
  me: ["user", "me"] as const,
};

export function useCurrentUserQuery() {
  return useQuery({
    queryKey: userKeys.me,
    queryFn: () => httpClient.get<CurrentUser>("/me"),
  });
}
