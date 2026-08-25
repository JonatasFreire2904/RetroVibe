import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { Squad, Template, Theme } from "@/shared/types";

export const catalogKeys = {
  templates: ["catalog", "templates"] as const,
  themes: ["catalog", "themes"] as const,
  squads: ["catalog", "squads"] as const,
};

export function useTemplatesQuery() {
  return useQuery({
    queryKey: catalogKeys.templates,
    queryFn: () => httpClient.get<Template[]>("/templates"),
    staleTime: 5 * 60 * 1000,
  });
}

export function useThemesQuery() {
  return useQuery({
    queryKey: catalogKeys.themes,
    queryFn: () => httpClient.get<Theme[]>("/themes"),
    staleTime: 5 * 60 * 1000,
  });
}

export function useSquadsQuery() {
  return useQuery({
    queryKey: catalogKeys.squads,
    queryFn: () => httpClient.get<Squad[]>("/squads"),
    staleTime: 60 * 1000,
  });
}
