import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuthToken } from "@/shared/lib/useAuthToken";
import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";

export function RequireAuth({ children }: { children: ReactNode }) {
  const token = useAuthToken();
  const location = useLocation();
  const pending = useQuery({ queryKey: ["pending-surveys"],
    queryFn: () => httpClient.get<{ sessionId: string; title: string | null }[]>("/me/pending-surveys"),
    enabled: Boolean(token), staleTime: 0 });

  if (!token) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (pending.isLoading) return <div className="flex min-h-screen items-center justify-center text-sm text-slate-500">Carregando…</div>;
  if (pending.isError) return <div className="flex min-h-screen items-center justify-center text-sm text-rose-600">Não foi possível verificar os questionários pendentes. Atualize a página.</div>;
  if (pending.data?.length && !location.pathname.startsWith("/pesquisa/"))
    return <Navigate to={`/pesquisa/${pending.data[0].sessionId}`} replace />;

  return <>{children}</>;
}
