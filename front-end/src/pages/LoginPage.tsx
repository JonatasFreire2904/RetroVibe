import { useState } from "react";
import type { FormEvent } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useLoginMutation } from "@/modules/auth/api/mutations";
import { useAuthToken } from "@/shared/lib/useAuthToken";
import { Button } from "@/shared/ui/Button";

export function LoginPage() {
  const token = useAuthToken();
  const navigate = useNavigate();
  const location = useLocation();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const login = useLoginMutation();

  if (token) {
    const redirectTo = (location.state as { from?: string } | null)?.from ?? "/";
    return <Navigate to={redirectTo} replace />;
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    login.mutate(
      { username: username.trim().toLowerCase(), password },
      { onSuccess: () => navigate("/", { replace: true }) }
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-violet-50 via-white to-white px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-gradient text-2xl shadow-sm shadow-fuchsia-200">
            🔁
          </div>
          <p className="mt-3 font-display text-xl font-extrabold text-slate-900">RetroVibe</p>
          <p className="font-mono text-[10px] font-medium tracking-widest text-violet-400">RETROSPECTIVE PLATFORM</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
          <div>
            <h1 className="text-lg font-bold text-slate-900">Entrar</h1>
            <p className="mt-1 text-sm text-slate-500">Acesse sua conta para continuar</p>
          </div>

          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-slate-600">Usuário</span>
            <input
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="login"
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 text-sm outline-none placeholder:text-slate-400 focus:border-violet-400"
            />
          </label>

          <label className="block">
            <span className="mb-1.5 block text-sm font-medium text-slate-600">Senha</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••"
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 text-sm outline-none placeholder:text-slate-400 focus:border-violet-400"
            />
          </label>

          {login.isError && <p className="text-sm text-rose-600">Usuário ou senha inválidos.</p>}

          <Button type="submit" className="w-full" disabled={!username.trim() || !password || login.isPending}>
            {login.isPending ? "Entrando…" : "Entrar"}
          </Button>
        </form>
      </div>
    </div>
  );
}
