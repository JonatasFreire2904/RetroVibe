import { useState, type FormEvent, type KeyboardEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSquadsQuery, useSquadMembersQuery, catalogKeys } from "@/modules/catalog/api/queries";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { httpClient, ApiError } from "@/shared/lib/httpClient";
import { Avatar } from "@/shared/ui/Avatar";
import { ConfirmDialog } from "@/shared/ui/ConfirmDialog";
import { XIcon } from "@/shared/ui/Icons";
import type { Squad, SquadMember } from "@/shared/types";

const inputClass = "w-full rounded-xl border border-slate-200 px-4 py-3 font-normal outline-none focus:border-violet-400";

function errorMessage(error: unknown, fallback: string) {
  return error instanceof ApiError ? error.message : fallback;
}

export function SquadsPage() {
  const [name, setName] = useState("");
  const [memberDraft, setMemberDraft] = useState("");
  const [members, setMembers] = useState<string[]>([]);
  const { data: squads = [] } = useSquadsQuery();
  const { data: user } = useCurrentUserQuery();
  const queryClient = useQueryClient();
  const create = useMutation({
    mutationFn: (body: { name: string; members: string[] }) => httpClient.post<Squad>("/squads", body),
    onSuccess: () => {
      setName(""); setMembers([]); setMemberDraft("");
      queryClient.invalidateQueries({ queryKey: catalogKeys.squads });
    }
  });
  function addDraftMember() {
    const member = memberDraft.trim();
    if (member && !members.some(m => m.toLowerCase() === member.toLowerCase())) setMembers([...members, member]);
    setMemberDraft("");
  }
  function onMemberKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Enter" || event.key === ",") { event.preventDefault(); addDraftMember(); }
  }
  function submit(event: FormEvent) {
    event.preventDefault();
    const pending = memberDraft.trim();
    const allMembers = pending && !members.some(m => m.toLowerCase() === pending.toLowerCase()) ? [...members, pending] : members;
    if (name.trim()) create.mutate({ name: name.trim(), members: allMembers });
  }
  return <div className="mx-auto max-w-4xl space-y-6">
    <div><h1 className="text-2xl font-extrabold text-slate-900">Squads</h1>
      <p className="mt-1 text-sm text-slate-500">Escolha uma das suas squads ao criar uma retrospectiva. Os membros cadastrados aqui aparecem como responsáveis nos itens de ação, sem precisar de conta.</p></div>
    {user?.accessLevel === "FACILITATOR" && <form onSubmit={submit} className="space-y-4 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <label className="block text-sm font-semibold text-slate-700">Criar squad
        <input value={name} onChange={event => setName(event.target.value)} maxLength={80}
          placeholder="Nome da nova squad" className={`mt-2 ${inputClass}`} />
      </label>
      <div className="text-sm font-semibold text-slate-700">Membros
        <div className="mt-2 flex gap-2">
          <input value={memberDraft} onChange={event => setMemberDraft(event.target.value)} onKeyDown={onMemberKeyDown} maxLength={80}
            placeholder="Nome do membro e Enter" className={inputClass} />
          <button type="button" onClick={addDraftMember} disabled={!memberDraft.trim()}
            className="rounded-xl border border-violet-200 px-4 text-sm font-semibold text-violet-700 disabled:opacity-50">Adicionar</button>
        </div>
        {members.length > 0 && <div className="mt-3 flex flex-wrap gap-2">
          {members.map(member => <span key={member} className="inline-flex items-center gap-1 rounded-full bg-violet-50 py-1 pl-3 pr-1 text-xs font-medium text-violet-700">
            {member}
            <button type="button" onClick={() => setMembers(members.filter(m => m !== member))} aria-label={`Remover ${member}`}
              className="rounded-full p-1 hover:bg-violet-100"><XIcon className="h-3 w-3" /></button>
          </span>)}
        </div>}
      </div>
      <button disabled={!name.trim() || create.isPending} className="rounded-xl bg-violet-600 px-5 py-3 text-sm font-semibold text-white disabled:opacity-50">Criar squad</button>
      {create.isError && <p className="text-sm text-rose-600">{errorMessage(create.error, "Não foi possível criar a squad.")}</p>}
    </form>}
    <div className="grid gap-3 sm:grid-cols-2">
      {squads.map(squad => <SquadCard key={squad.id} squad={squad} />)}
      {squads.length === 0 && <p className="text-sm text-slate-500">Nenhuma squad disponível.</p>}
    </div>
  </div>;
}

function SquadCard({ squad }: { squad: Squad }) {
  const [draft, setDraft] = useState("");
  const [memberToRemove, setMemberToRemove] = useState<SquadMember | null>(null);
  const { data: members = [], isLoading } = useSquadMembersQuery(squad.id);
  const queryClient = useQueryClient();
  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: catalogKeys.squadMembers(squad.id) });
    queryClient.invalidateQueries({ queryKey: ["sessions", "assignees"] });
  };
  const add = useMutation({
    mutationFn: (memberName: string) => httpClient.post<SquadMember>(`/squads/${squad.id}/members`, { name: memberName }),
    onSuccess: () => { setDraft(""); refresh(); }
  });
  const remove = useMutation({
    mutationFn: (memberId: string) => httpClient.delete<void>(`/squads/${squad.id}/members/${memberId}`),
    onSuccess: () => { setMemberToRemove(null); refresh(); }
  });
  function submit(event: FormEvent) {
    event.preventDefault();
    if (draft.trim()) add.mutate(draft.trim());
  }
  return <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
    <p className="font-bold text-slate-800">{squad.name}</p>
    <p className="mt-1 text-xs text-slate-400">{members.length === 1 ? "1 membro" : `${members.length} membros`}</p>
    <ul className="mt-3 space-y-2">
      {members.map(member => <li key={member.id} className="flex items-center gap-2 text-sm text-slate-700">
        <Avatar name={member.name} color={member.avatarColor} size="sm" />
        <span className="flex-1 truncate">{member.name}</span>
        <button type="button" onClick={() => setMemberToRemove(member)} aria-label={`Remover ${member.name}`}
          className="rounded-full p-1 text-slate-400 hover:bg-rose-50 hover:text-rose-600"><XIcon className="h-3.5 w-3.5" /></button>
      </li>)}
      {!isLoading && members.length === 0 && <li className="text-xs text-slate-400">Nenhum membro cadastrado.</li>}
    </ul>
    <form onSubmit={submit} className="mt-3 flex gap-2">
      <input value={draft} onChange={event => setDraft(event.target.value)} maxLength={80} placeholder="Adicionar membro"
        className="min-w-0 flex-1 rounded-xl border border-slate-200 px-3 py-2 text-sm outline-none focus:border-violet-400" />
      <button disabled={!draft.trim() || add.isPending} className="rounded-xl bg-violet-600 px-3 py-2 text-sm font-semibold text-white disabled:opacity-50">Adicionar</button>
    </form>
    {add.isError && <p className="mt-2 text-xs text-rose-600">{errorMessage(add.error, "Não foi possível adicionar o membro.")}</p>}
    <ConfirmDialog
      open={memberToRemove !== null}
      title="Remover membro"
      message={`Remover ${memberToRemove?.name ?? ""} da squad ${squad.name}? Ele deixa de aparecer como responsável, mas os itens de ação já atribuídos continuam com o nome.`}
      confirmLabel="Remover"
      pendingLabel="Removendo…"
      tone="danger"
      isPending={remove.isPending}
      error={remove.isError ? errorMessage(remove.error, "Não foi possível remover o membro.") : null}
      onConfirm={() => memberToRemove && remove.mutate(memberToRemove.id)}
      onCancel={() => { remove.reset(); setMemberToRemove(null); }}
    />
  </div>;
}
