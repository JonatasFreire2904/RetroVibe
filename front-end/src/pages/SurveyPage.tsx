import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, Navigate, useNavigate, useParams } from "react-router-dom";
import { httpClient, ApiError } from "@/shared/lib/httpClient";
import { getParticipantMeta } from "@/shared/lib/participantMeta";
import { getParticipantToken } from "@/shared/lib/participantToken";

interface SurveyStatus { enabled: boolean; open: boolean; completed: boolean; role: "FACILITATOR" | "PARTICIPANT" }

export function SurveyPage({ guest = false }: { guest?: boolean }) {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [teamMoreEngaged, setTeamMoreEngaged] = useState<boolean | null>(null);
  const [usedCustomTheme, setUsedCustomTheme] = useState<boolean | null>(null);
  const [engagementScore, setEngagement] = useState(0);
  const [usabilityScore, setUsability] = useState(0);
  const [suggestion, setSuggestion] = useState("");
  const meta = guest ? getParticipantMeta() : null;
  const hasGuestAccess = !guest || (meta?.sessionId === sessionId && Boolean(getParticipantToken()));
  const status = useQuery({ queryKey: ["survey", sessionId, guest],
    queryFn: () => httpClient.get<SurveyStatus>(`/sessions/${sessionId}/survey`),
    enabled: Boolean(sessionId && hasGuestAccess), refetchOnWindowFocus: false });
  const submit = useMutation({ mutationFn: () => httpClient.post(`/sessions/${sessionId}/survey`, {
    teamMoreEngaged: result?.role === "FACILITATOR" ? teamMoreEngaged : null,
    usedCustomTheme: result?.role === "FACILITATOR" ? usedCustomTheme : null,
    engagementScore, usabilityScore, suggestion: suggestion.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ["survey", sessionId] });
      queryClient.invalidateQueries({ queryKey: ["pending-surveys"] });
      if (!guest) navigate("/", { replace: true }); }
  });
  if (!hasGuestAccess) return <Navigate to={`/entrar/${sessionId ?? ""}`} replace />;
  const result = status.data;
  const completed = result?.completed || submit.isSuccess;
  const canSubmit = Boolean(engagementScore && usabilityScore && suggestion.trim() &&
    (result?.role !== "FACILITATOR" || (teamMoreEngaged !== null && usedCustomTheme !== null)));
  function handleSubmit(event: FormEvent) { event.preventDefault(); if (canSubmit) submit.mutate(); }
  return <div className="min-h-screen bg-gradient-to-b from-violet-100 via-white to-rose-50 px-4 py-10">
    <div className="mx-auto max-w-xl rounded-3xl border border-violet-100 bg-white p-6 shadow-xl sm:p-8">
      <p className="text-xs font-bold uppercase tracking-widest text-violet-500">RetroVibe · Pesquisa</p>
      <h1 className="mt-2 text-2xl font-extrabold text-slate-900">Avalie esta retrospectiva</h1>
      <p className="mt-2 text-sm text-slate-500">Sua resposta é necessária para concluir sua participação. Responda a todas as perguntas; se não tiver uma sugestão, escreva "Nenhuma". As notas vão de 1 (muito pouco) a 5 (muito).</p>
      {status.isLoading && <p className="mt-6 text-sm text-slate-500">Carregando questionário…</p>}
      {status.isError && <p className="mt-6 text-sm text-rose-600">Não foi possível abrir o questionário. Atualize a página e tente novamente.</p>}
      {completed && <div className="mt-7 rounded-2xl bg-emerald-50 p-5 text-emerald-800"><strong>Obrigado por responder!</strong>
        <p className="mt-1 text-sm">Sua avaliação foi registrada.</p>{!guest && <Link to="/" className="mt-3 inline-block font-semibold underline">Voltar à Home</Link>}</div>}
      {result && !result.open && !completed && <p className="mt-6 text-sm text-slate-600">O questionário será liberado quando o facilitador encerrar a sessão.</p>}
      {result?.open && !completed && <form className="mt-7 space-y-7" onSubmit={handleSubmit}>
        {result.role === "FACILITATOR" ? <div className="space-y-6">
          <h2 className="text-sm font-bold uppercase tracking-wide text-violet-700">QP1 · Engajamento</h2>
          <YesNoQuestion label="Você sentiu a equipe mais engajada na cerimônia com o uso do software RetroVibe?"
            value={teamMoreEngaged} onChange={setTeamMoreEngaged} />
          <YesNoQuestion label="Você fez uso de algum tema personalizado (Natal, Páscoa, etc.) nessa sessão?"
            value={usedCustomTheme} onChange={setUsedCustomTheme} />
          <Rating label="De 1 a 5, qual nota você dá para o engajamento do time nessa cerimônia?"
            value={engagementScore} onChange={setEngagement} />
        </div> : <div className="space-y-6">
          <h2 className="text-sm font-bold uppercase tracking-wide text-violet-700">QP1 · Engajamento</h2>
          <Rating label="De 1 a 5, qual nota você dá para o seu engajamento nessa cerimônia?"
            value={engagementScore} onChange={setEngagement} anonymous />
        </div>}
        <div className="space-y-4"><h2 className="text-sm font-bold uppercase tracking-wide text-violet-700">QP2 · Usabilidade</h2>
          <Rating label={result.role === "FACILITATOR"
            ? "De 1 a 5, qual nota você dá para a usabilidade do RetroVibe?"
            : "De 1 a 5, qual nota você dá para a usabilidade do sistema utilizado nessa Retrospectiva?"}
            value={usabilityScore} onChange={setUsability} anonymous={result.role === "PARTICIPANT"} />
        </div>
        <div className="rounded-2xl bg-violet-50 p-4 text-sm text-violet-800">
          <h2 className="font-bold">QP3 · Dados levantados pelo próprio software</h2>
          <p className="mt-1">O sistema registra automaticamente os dados desta sessão. Não há nada para preencher aqui.</p>
        </div>
        <div><h2 className="mb-3 text-sm font-bold uppercase tracking-wide text-violet-700">QP4 · Sugestões</h2>
        <label className="block text-sm font-semibold text-slate-800">Você tem alguma sugestão de melhoria no tema ou layout utilizado nessa cerimônia? <span className="text-rose-500">*</span>
          <textarea value={suggestion} onChange={event => setSuggestion(event.target.value)} maxLength={2000} rows={4}
            required placeholder="Se não tiver sugestões, escreva Nenhuma." className="mt-2 w-full rounded-xl border border-slate-200 p-3 font-normal outline-none focus:border-violet-400" />
        </label>
        {result.role === "PARTICIPANT" && <AnonymousNote />}</div>
        {submit.isError && <p className="text-sm text-rose-600">{submit.error instanceof ApiError ? submit.error.message : "Não foi possível enviar. Tente novamente."}</p>}
        <button disabled={!canSubmit || submit.isPending}
          className="w-full rounded-xl bg-violet-600 px-5 py-3 font-bold text-white disabled:opacity-50">
          {submit.isPending ? "Enviando…" : "Enviar avaliação"}
        </button>
      </form>}
    </div>
  </div>;
}

function AnonymousNote() {
  return <p className="mt-2 text-xs text-slate-500">Essa informação será compartilhada de forma anônima com o criador da sessão.</p>;
}

function YesNoQuestion({ label, value, onChange }: { label: string; value: boolean | null; onChange: (value: boolean) => void }) {
  return <fieldset><legend className="mb-3 text-sm font-semibold text-slate-800">{label} <span className="text-rose-500">*</span></legend>
    <div className="flex gap-2">{[true, false].map(answer => <button key={String(answer)} type="button"
      aria-pressed={value === answer} onClick={() => onChange(answer)}
      className={`rounded-xl border px-5 py-2 text-sm font-semibold transition ${value === answer ? "border-violet-500 bg-violet-100 text-violet-800" : "border-slate-200 text-slate-500 hover:border-violet-300"}`}>
      {answer ? "Sim" : "Não"}</button>)}</div>
  </fieldset>;
}

function Rating({ label, value, onChange, anonymous = false }: { label: string; value: number; onChange: (value: number) => void; anonymous?: boolean }) {
  return <fieldset><legend className="mb-3 text-sm font-semibold text-slate-800">{label} <span className="text-rose-500">*</span></legend>
    <div className="flex flex-wrap gap-2">{[1, 2, 3, 4, 5].map(score => <button key={score} type="button" aria-label={`${score} de 5`}
      aria-pressed={value === score} onClick={() => onChange(score)}
      className={`rounded-xl border px-3 py-2 text-lg transition ${value === score ? "border-violet-500 bg-violet-100 text-violet-800" : "border-slate-200 text-slate-400 hover:border-violet-300"}`}>
      {score} ★</button>)}</div>
    {anonymous && <AnonymousNote />}
  </fieldset>;
}
