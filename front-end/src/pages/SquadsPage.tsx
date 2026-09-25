import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSquadsQuery, catalogKeys } from "@/modules/catalog/api/queries";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { httpClient, ApiError } from "@/shared/lib/httpClient";
import type { Squad } from "@/shared/types";

export function SquadsPage() {
  const [name, setName] = useState("");
  const { data: squads = [] } = useSquadsQuery();
  const { data: user } = useCurrentUserQuery();
  const queryClient = useQueryClient();
  const create = useMutation({
    mutationFn: (squadName: string) => httpClient.post<Squad>("/squads", { name: squadName }),
    onSuccess: () => { setName(""); queryClient.invalidateQueries({ queryKey: catalogKeys.squads }); }
  });
  function submit(event: FormEvent) {
    event.preventDefault();
    if (name.trim()) create.mutate(name.trim());
  }
  return <div className="mx-auto max-w-4xl space-y-6">
    <div><h1 className="text-2xl font-extrabold text-slate-900">Squads</h1>
      <p className="mt-1 text-sm text-slate-500">Escolha uma das suas squads ao criar uma retrospectiva.</p></div>
    {user?.accessLevel === "FACILITATOR" && <form onSubmit={submit} className="flex flex-wrap gap-3 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <label className="min-w-56 flex-1 text-sm font-semibold text-slate-700">Criar squad
        <input value={name} onChange={event => setName(event.target.value)} maxLength={80}
          placeholder="Nome da nova squad" className="mt-2 w-full rounded-xl border border-slate-200 px-4 py-3 font-normal outline-none focus:border-violet-400" />
      </label>
      <button disabled={!name.trim() || create.isPending} className="self-end rounded-xl bg-violet-600 px-5 py-3 text-sm font-semibold text-white disabled:opacity-50">Criar squad</button>
      {create.isError && <p className="w-full text-sm text-rose-600">{create.error instanceof ApiError ? create.error.message : "Não foi possível criar a squad."}</p>}
    </form>}
    <div className="grid gap-3 sm:grid-cols-2">
      {squads.map(squad => <div key={squad.id} className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
        <p className="font-bold text-slate-800">{squad.name}</p><p className="mt-1 text-xs text-slate-400">Squad disponível para sessões</p>
      </div>)}
      {squads.length === 0 && <p className="text-sm text-slate-500">Nenhuma squad disponível.</p>}
    </div>
  </div>;
}
