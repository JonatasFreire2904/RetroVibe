import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useJoinSessionMutation } from "@/modules/sessions/api/mutations";
import { ApiError } from "@/shared/lib/httpClient";
import { Button } from "@/shared/ui/Button";

export function JoinSessionPage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const [displayName, setDisplayName] = useState("");
  const join = useJoinSessionMutation(sessionId ?? "");

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!sessionId) return;
    join.mutate(displayName.trim(), {
      onSuccess: () => navigate(`/participar/${sessionId}`, { replace: true }),
    });
  }

  const errorMessage =
    join.error instanceof ApiError
      ? join.error.status === 409
        ? "Esta sessão não está mais ativa."
        : join.error.status === 404
          ? "Sessão não encontrada — confira o link."
          : "Não foi possível entrar na sessão."
      : null;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-violet-50 via-white to-white px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-gradient text-2xl shadow-sm shadow-fuchsia-200">
            🔁
          </div>
          <p className="mt-3 font-display text-xl font-extrabold text-slate-900">RetroVibe</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
          <div>
            <h1 className="text-lg font-bold text-slate-900">Entrar na retrospectiva</h1>
            <p className="mt-1 text-sm text-slate-500">Digite seu nome para participar — sem cadastro.</p>
          </div>

          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-slate-600">Seu nome</span>
            <input
              autoFocus
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Como podemos te chamar?"
              maxLength={60}
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 text-sm outline-none placeholder:text-slate-400 focus:border-violet-400"
            />
          </label>

          {errorMessage && <p className="text-sm text-rose-600">{errorMessage}</p>}

          <Button type="submit" className="w-full" disabled={!displayName.trim() || join.isPending}>
            {join.isPending ? "Entrando…" : "Entrar"}
          </Button>
        </form>
      </div>
    </div>
  );
}
