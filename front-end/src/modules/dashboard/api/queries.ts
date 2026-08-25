import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { TeamDashboard } from "@/shared/types";

export function useTeamDashboardQuery(squadId?: string) {
  return useQuery({
    queryKey: ["dashboard", squadId ?? "all"] as const,
    queryFn: () => httpClient.get<TeamDashboard>("/dashboard", { squadId }),
  });
}
