import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { Template, Theme } from "@/shared/types";

export const adminCatalogKeys = {
  templates: ["catalog", "admin", "templates"] as const,
  themes: ["catalog", "admin", "themes"] as const,
};

export function useAdminTemplatesQuery() {
  return useQuery({
    queryKey: adminCatalogKeys.templates,
    queryFn: () => httpClient.get<Template[]>("/admin/templates"),
  });
}

export function useAdminThemesQuery() {
  return useQuery({
    queryKey: adminCatalogKeys.themes,
    queryFn: () => httpClient.get<Theme[]>("/admin/themes"),
  });
}
