import { useMutation, useQueryClient } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import type { Template, Theme } from "@/shared/types";
import { catalogKeys } from "./queries";
import { adminCatalogKeys } from "./adminQueries";

function invalidateCatalog(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: adminCatalogKeys.templates });
  queryClient.invalidateQueries({ queryKey: adminCatalogKeys.themes });
  queryClient.invalidateQueries({ queryKey: catalogKeys.templates });
  queryClient.invalidateQueries({ queryKey: catalogKeys.themes });
}

export interface CreateTemplateInput {
  key?: string;
  label: string;
  icon: string;
  description: string;
  columns: { key: string; label: string; icon: string }[];
}

export interface UpdateTemplateInput {
  id: string;
  label?: string;
  icon?: string;
  description?: string;
  active?: boolean;
}

export function useCreateTemplateMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateTemplateInput) => httpClient.post<Template>("/admin/templates", input),
    onSuccess: () => invalidateCatalog(queryClient),
  });
}

export function useUpdateTemplateMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...input }: UpdateTemplateInput) => httpClient.patch<Template>(`/admin/templates/${id}`, input),
    onSuccess: () => invalidateCatalog(queryClient),
  });
}

export interface CreateThemeInput {
  key?: string;
  label: string;
  emoji: string;
}

export interface UpdateThemeInput {
  id: string;
  label?: string;
  emoji?: string;
  active?: boolean;
}

export function useCreateThemeMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: CreateThemeInput) => httpClient.post<Theme>("/admin/themes", input),
    onSuccess: () => invalidateCatalog(queryClient),
  });
}

export function useUpdateThemeMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...input }: UpdateThemeInput) => httpClient.patch<Theme>(`/admin/themes/${id}`, input),
    onSuccess: () => invalidateCatalog(queryClient),
  });
}
