import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { httpClient, ApiError } from "@/shared/lib/httpClient";
import { useCurrentUserQuery } from "@/modules/user/api/queries";

type Mode = "real" | "test" | "all";
interface Squad { id: string; name: string }
interface Facilitator { id: string; name: string; username: string; isTest: boolean; squads: Squad[] }
interface Overview { sessions: number; expectedResponses: number; receivedResponses: number; completionPercent: number;
  engagementAverage: number; usabilityAverage: number; cards: number; votes: number; actionItems: number;
  facilitator: RoleStats; participants: RoleStats;
  scoreDistribution: { score: number; engagement: number; usability: number }[];
  suggestions: { respondentRole: string; suggestion: string }[] }
interface RoleStats { count: number; engagementAverage: number; usabilityAverage: number }
interface FacilitatorSquads { facilitator: { id: string; name: string; isTest: boolean };
  squads: (Squad & { sessions: number })[] }
interface ResearchSession { id: string; title: string | null; status: string; createdAt: string;
  isTest: boolean; expectedResponses: number; receivedResponses: number }
interface SessionDetail { id: string; title: string | null; isTest: boolean; squad: Squad;
  facilitator: { id: string; name: string } | null; expectedResponses: number; receivedResponses: number;
  engagementAverage: number; usabilityAverage: number;
  metrics: { participants: number; cards: number; votes: number; actionItems: number; durationMinutes: number };
  respondents: { name: string; role: string; completed: boolean }[];
  responses: { number: number; respondentRole: string; engagementScore: number; usabilityScore: number;
    suggestion: string }[] }

export function AdminResearchPage() {
  const { data: user } = useCurrentUserQuery();
  const queryClient = useQueryClient();
  const [mode, setMode] = useState<Mode>("real");
  const [facilitatorMode, setFacilitatorMode] = useState<Mode>("all");
  const [facilitatorId, setFacilitatorId] = useState<string | null>(null);
  const [squadId, setSquadId] = useState<string | null>(null);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [newIsTest, setNewIsTest] = useState(false);
  const [resetFor, setResetFor] = useState<string | null>(null);
  const [resetPassword, setResetPassword] = useState("");
  const facilitators = useQuery({ queryKey: ["admin", "facilitators"],
    queryFn: () => httpClient.get<Facilitator[]>("/admin/facilitators"), enabled: user?.accessLevel === "ADMIN" });
  const overview = useQuery({ queryKey: ["research", "overview", mode],
    queryFn: () => httpClient.get<Overview>("/research/overview", { mode }), enabled: user?.accessLevel === "ADMIN" });
  const facilitatorSquads = useQuery({ queryKey: ["research", "facilitator", facilitatorId, mode],
    queryFn: () => httpClient.get<FacilitatorSquads>(`/research/facilitators/${facilitatorId}/squads`, { mode }),
    enabled: Boolean(facilitatorId) });
  const sessions = useQuery({ queryKey: ["research", "squad", squadId, facilitatorId, mode],
    queryFn: () => httpClient.get<ResearchSession[]>(`/research/squads/${squadId}/sessions`, { mode, facilitatorId: facilitatorId ?? undefined }),
    enabled: Boolean(squadId) });
  const detail = useQuery({ queryKey: ["research", "session", sessionId],
    queryFn: () => httpClient.get<SessionDetail>(`/research/sessions/${sessionId}`), enabled: Boolean(sessionId) });
  const create = useMutation({ mutationFn: () => httpClient.post<Facilitator>("/admin/facilitators", { name, username, password, isTest: newIsTest }),
    onSuccess: () => { setName(""); setUsername(""); setPassword(""); setNewIsTest(false);
      queryClient.invalidateQueries({ queryKey: ["admin", "facilitators"] }); } });
  const reset = useMutation({ mutationFn: (id: string) => httpClient.patch(`/admin/facilitators/${id}/password`, { password: resetPassword }),
    onSuccess: () => { setResetFor(null); setResetPassword(""); } });
  const toggleTest = useMutation({ mutationFn: ({ id, isTest }: { id: string; isTest: boolean }) =>
    httpClient.patch(`/admin/facilitators/${id}/test`, { isTest }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin", "facilitators"] }) });
  if (user && user.accessLevel !== "ADMIN") return <p className="p-8 text-rose-600">Área exclusiva do administrador.</p>;
  function submit(event: FormEvent) { event.preventDefault(); create.mutate(); }
  function selectFacilitator(id: string) { setFacilitatorId(id); setSquadId(null); setSessionId(null); }
  return <div className="mx-auto max-w-6xl space-y-6 pb-12">
    <div><h1 className="text-2xl font-extrabold text-slate-900">Pesquisa · Administração</h1>
      <p className="mt-1 text-sm text-slate-500">Facilitadores, squads, retrospectivas e respostas em um só lugar.</p></div>
    <div className="flex flex-wrap items-center gap-3 rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
      <label className="text-sm font-semibold text-slate-700">Dados exibidos <select value={mode} onChange={event => setMode(event.target.value as Mode)}
        className="ml-3 rounded-xl border border-slate-200 px-3 py-2 font-normal"><option value="real">Só pesquisa real</option>
        <option value="test">Só testes</option><option value="all">Reais e testes</option></select></label>
      <span className="text-xs text-slate-500">A classificação pertence à sessão e não muda retroativamente.</span>
    </div>
    {overview.data && <div className="space-y-4">
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <Metric label="Retros encerradas" value={overview.data.sessions} />
        <Metric label="Questionários recebidos" value={`${overview.data.receivedResponses}/${overview.data.expectedResponses}`} />
        <Metric label="Taxa de resposta" value={`${overview.data.completionPercent}%`} />
        <Metric label="Cards criados" value={overview.data.cards} />
      </div>
      <div className="grid gap-3 sm:grid-cols-2"><RoleCard title="Facilitadores" stats={overview.data.facilitator} />
        <RoleCard title="Convidados" stats={overview.data.participants} /></div>
      <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
        <h2 className="font-bold text-slate-800">Distribuição das notas</h2>
        <div className="mt-3 grid grid-cols-5 gap-2 text-center text-xs">{overview.data.scoreDistribution.map(row => <div key={row.score} className="rounded-xl bg-violet-50 p-3">
          <strong className="text-violet-700">{row.score} ★</strong><p className="mt-2">Engajamento: {row.engagement}</p>
          <p>Usabilidade: {row.usability}</p></div>)}</div>
      </div>
      <div className="grid gap-3 sm:grid-cols-2"><Metric label="Votos registrados" value={overview.data.votes} />
        <Metric label="Itens de ação" value={overview.data.actionItems} /></div>
      {overview.data.suggestions.length > 0 && <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
        <h2 className="font-bold text-slate-800">Sugestões recebidas</h2>
        <div className="mt-3 space-y-2">{overview.data.suggestions.map((item, index) => <p key={index} className="rounded-xl bg-slate-50 p-3 text-sm text-slate-700">
          <span className="mr-2 text-xs font-bold text-violet-600">{item.respondentRole === "FACILITATOR" ? "Facilitador" : "Convidado"}</span>{item.suggestion}</p>)}</div>
      </div>}
    </div>}
    <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <h2 className="font-bold text-slate-800">Criar facilitador</h2>
      <form onSubmit={submit} className="mt-4 grid gap-3 sm:grid-cols-3">
        <input aria-label="Nome do facilitador" placeholder="Nome" value={name} onChange={event => setName(event.target.value)} required className="rounded-xl border border-slate-200 px-3 py-2" />
        <input aria-label="Usuário do facilitador" placeholder="Usuário" value={username} onChange={event => setUsername(event.target.value)} required className="rounded-xl border border-slate-200 px-3 py-2" />
        <input aria-label="Senha inicial" placeholder="Senha inicial (12+ caracteres)" type="password" minLength={12} value={password} onChange={event => setPassword(event.target.value)} required className="rounded-xl border border-slate-200 px-3 py-2" />
        <label className="flex items-center gap-2 text-sm text-slate-600"><input type="checkbox" checked={newIsTest} onChange={event => setNewIsTest(event.target.checked)} /> Facilitador de teste</label>
        <button disabled={create.isPending} className="rounded-xl bg-violet-600 px-4 py-2 font-semibold text-white disabled:opacity-50">{create.isPending ? "Criando…" : "Criar facilitador"}</button>
        {create.isError && <p className="text-sm text-rose-600">{create.error instanceof ApiError ? create.error.message : "Não foi possível criar."}</p>}
      </form>
    </section>
    <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-3"><h2 className="font-bold text-slate-800">Facilitadores</h2>
        <label className="text-sm text-slate-600">Tipo de facilitador <select value={facilitatorMode}
          onChange={event => { setFacilitatorMode(event.target.value as Mode); setFacilitatorId(null); setSquadId(null); setSessionId(null); }}
          className="ml-2 rounded-lg border border-slate-200 px-2 py-1"><option value="all">Todos</option>
          <option value="real">Só reais</option><option value="test">Só testes</option></select></label></div>
      <div className="mt-4 grid gap-3 sm:grid-cols-2">{facilitators.data?.filter(f => facilitatorMode === "all" || f.isTest === (facilitatorMode === "test")).map(f => <div key={f.id} className={`rounded-xl border p-4 ${facilitatorId === f.id ? "border-violet-400 bg-violet-50" : "border-slate-200"}`}>
        <button onClick={() => selectFacilitator(f.id)} className="text-left font-bold text-violet-700 hover:underline">{f.name} →</button>
        <p className="text-xs text-slate-500">@{f.username} · {f.squads.length} squads · {f.isTest ? "Teste" : "Pesquisa real"}</p>
        <div className="mt-3 flex flex-wrap gap-2">
          <button onClick={() => toggleTest.mutate({ id: f.id, isTest: !f.isTest })} className="rounded-lg border border-slate-200 px-2 py-1 text-xs">{f.isTest ? "Desmarcar teste" : "Marcar como teste"}</button>
          <button onClick={() => { setResetFor(f.id); setResetPassword(""); }} className="rounded-lg border border-slate-200 px-2 py-1 text-xs">Redefinir senha</button>
        </div>
        {resetFor === f.id && <div className="mt-3 flex flex-wrap gap-2"><input type="password" aria-label={`Nova senha de ${f.name}`} minLength={12} value={resetPassword}
          onChange={event => setResetPassword(event.target.value)} placeholder="Nova senha (12+ caracteres)" className="min-w-44 flex-1 rounded-lg border border-slate-200 px-2 py-1 text-sm" />
          <button disabled={resetPassword.length < 12 || reset.isPending} onClick={() => reset.mutate(f.id)} className="rounded-lg bg-violet-600 px-3 py-1 text-xs font-bold text-white disabled:opacity-50">Salvar nova senha</button>
          <button onClick={() => setResetFor(null)} className="text-xs text-slate-500">Cancelar</button>
          {reset.isError && <p className="w-full text-xs text-rose-600">Não foi possível redefinir a senha.</p>}</div>}
      </div>)}</div>
    </section>
    {facilitatorId && <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <h2 className="font-bold text-slate-800">Squads de {facilitatorSquads.data?.facilitator.name ?? "…"}</h2>
      <div className="mt-3 grid gap-3 sm:grid-cols-2">{facilitatorSquads.data?.squads.map(squad => <button key={squad.id}
        onClick={() => { setSquadId(squad.id); setSessionId(null); }} className={`rounded-xl border p-4 text-left ${squadId === squad.id ? "border-violet-400 bg-violet-50" : "border-slate-200"}`}>
        <strong>{squad.name}</strong><span className="ml-2 text-xs text-slate-500">{squad.sessions} sessões →</span></button>)}</div>
    </section>}
    {squadId && <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <h2 className="font-bold text-slate-800">Retrospectivas da squad</h2>
      <div className="mt-3 space-y-2">{sessions.data?.map(session => <button key={session.id} onClick={() => setSessionId(session.id)}
        className={`flex w-full flex-wrap items-center justify-between rounded-xl border p-4 text-left ${sessionId === session.id ? "border-violet-400 bg-violet-50" : "border-slate-200"}`}>
        <span className="font-semibold">{session.title || "Retrospectiva"} {session.isTest && <small className="text-amber-600">· Teste</small>}</span>
        <span className="text-xs text-slate-500">{new Date(session.createdAt).toLocaleDateString("pt-BR")} · {session.receivedResponses}/{session.expectedResponses} respostas →</span>
      </button>)}{sessions.data?.length === 0 && <p className="text-sm text-slate-500">Nenhuma sessão neste filtro.</p>}</div>
    </section>}
    {sessionId && detail.data && <section className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
      <h2 className="text-lg font-bold text-slate-800">{detail.data.title || "Retrospectiva"} · Questionários</h2>
      <p className="mt-1 text-sm text-slate-500">{detail.data.squad.name} · Facilitador: {detail.data.facilitator?.name ?? "—"} · {detail.data.receivedResponses}/{detail.data.expectedResponses} respostas</p>
      <div className="mt-4 grid gap-3 sm:grid-cols-2"><Metric label="Engajamento" value={`${detail.data.engagementAverage}/5`} />
        <Metric label="Usabilidade" value={`${detail.data.usabilityAverage}/5`} /></div>
      <h3 className="mt-6 font-bold text-slate-800">Dados levantados pelo software</h3>
      <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        <Metric label="Pessoas" value={detail.data.metrics.participants} />
        <Metric label="Cards" value={detail.data.metrics.cards} />
        <Metric label="Votos" value={detail.data.metrics.votes} />
        <Metric label="Itens de ação" value={detail.data.metrics.actionItems} />
        <Metric label="Duração" value={`${detail.data.metrics.durationMinutes} min`} />
      </div>
      <h3 className="mt-6 font-bold text-slate-800">Preenchimento</h3>
      <div className="mt-2 flex flex-wrap gap-2">{detail.data.respondents?.map((r, index) => <span key={index} className={`rounded-full px-3 py-1 text-xs ${r.completed ? "bg-emerald-50 text-emerald-700" : "bg-amber-50 text-amber-700"}`}>
        {r.name} · {r.role === "FACILITATOR" ? "Facilitador" : "Convidado"} · {r.completed ? "Respondido" : "Pendente"}</span>)}</div>
      <h3 className="mt-6 font-bold text-slate-800">Respostas individuais</h3>
      <div className="mt-3 grid gap-3 md:grid-cols-2">{detail.data.responses?.map(r => <article key={r.number} className="rounded-xl border border-slate-200 p-4 text-sm">
        <p className="font-bold text-violet-700">Questionário #{r.number} · {r.respondentRole === "FACILITATOR" ? "Facilitador" : "Convidado"}</p>
        <p className="mt-2">Engajamento: {r.engagementScore}/5 · Usabilidade: {r.usabilityScore}/5</p>
        <p className="mt-2 text-slate-600">{r.suggestion || "Sem sugestão de melhoria."}</p></article>)}</div>
    </section>}
  </div>;
}

function Metric({ label, value }: { label: string; value: string | number }) {
  return <div className="rounded-2xl border border-violet-100 bg-white p-4 shadow-sm"><p className="text-xs text-slate-500">{label}</p>
    <p className="mt-1 text-xl font-extrabold text-violet-700">{value}</p></div>;
}
function RoleCard({ title, stats }: { title: string; stats: RoleStats }) {
  return <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm"><h3 className="font-bold text-slate-800">{title} · {stats.count} respostas</h3>
    <p className="mt-3 text-sm text-slate-600">Engajamento {stats.engagementAverage}/5 · Usabilidade {stats.usabilityAverage}/5</p></div>;
}
