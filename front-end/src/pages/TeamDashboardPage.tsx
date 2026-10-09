import { useState } from "react";
import { useSquadsQuery } from "@/modules/catalog/api/queries";
import { useTeamDashboardQuery } from "@/modules/dashboard/api/queries";
import { BarRow } from "@/modules/dashboard/components/BarRow";
import { ParticipantsLineChart } from "@/modules/dashboard/components/ParticipantsLineChart";
import { StatCard } from "@/modules/dashboard/components/StatCard";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { useQuery } from "@tanstack/react-query";
import { httpClient } from "@/shared/lib/httpClient";
import { Card } from "@/shared/ui/Card";
import { CalendarIcon, ClockIcon, StarIcon, UsersIcon } from "@/shared/ui/Icons";
import { questionText, susQuestions, teamQuestions, uesQuestions } from "@/shared/surveyQuestionnaire";

export function TeamDashboardPage() {
  const [squadId, setSquadId] = useState("");
  const { data: currentUser } = useCurrentUserQuery();
  const isAdmin = currentUser?.accessLevel === "ADMIN";
  const researchMode = currentUser?.accessLevel === "FACILITATOR" && currentUser.isTest ? "all" : "real";
  const { data: squads = [] } = useSquadsQuery();
  const { data: dashboard, isLoading } = useTeamDashboardQuery(squadId || undefined);
  const { data: research } = useQuery({ queryKey: ["research", "overview", researchMode, squadId],
    queryFn: () => httpClient.get<{ sessions: number; expectedResponses: number; receivedResponses: number;
      completionPercent: number; engagementAverage: number; usabilityAverage: number; cards: number;
      facilitator: { count: number; engagementAverage: number; usabilityAverage: number };
      participants: { count: number; engagementAverage: number; usabilityAverage: number };
      teamMoreEngaged: { answered: number; yes: number; no: number; percentYes: number };
      customThemeUse: { answered: number; yes: number; no: number; percentYes: number };
      questionnaire: { count: number;
        facilitator: { count: number; susAverage: number | null; uesAverage: number | null; teamAverage: number | null;
          questions: { number: number; answered: number; average: number | null }[] };
        participants: { count: number; susAverage: number | null; uesAverage: number | null;
          questions: { number: number; answered: number; average: number | null }[] };
        customThemeUse: { answered: number; yes: number; no: number; percentYes: number } };
      suggestions: { respondentRole: string; suggestion: string }[] }>(
        "/research/overview", { mode: researchMode, squadId: squadId || undefined }), enabled: Boolean(currentUser) });
  const phaseTotal = dashboard ? dashboard.phaseAverages.collectMinutes + dashboard.phaseAverages.voteMinutes + dashboard.phaseAverages.discussMinutes : 0;
  const phaseLabel = (minutes: number) => `${minutes} min · ${phaseTotal > 0 ? Math.round(minutes / phaseTotal * 100) : 0}%`;

  return (
    <div className="mx-auto max-w-6xl">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-slate-900">Dashboard do Time</h1>
          <p className="mt-1 text-sm text-slate-500">
            {isAdmin ? "Métricas agregadas das retrospectivas" : `Métricas do squad ${squads.find(squad => squad.id === squadId)?.name ?? currentUser?.squad ?? ""}`}
          </p>
        </div>
        {(isAdmin || squads.length > 1) && (
          <select
            value={squadId}
            onChange={(e) => setSquadId(e.target.value)}
            className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-600 shadow-sm outline-none focus:border-violet-400"
          >
            <option value="">{isAdmin ? "Todos os Squads" : "Squad principal"}</option>
            {squads.map((squad) => (
              <option key={squad.id} value={squad.id}>
                {squad.name}
              </option>
            ))}
          </select>
        )}
      </div>

      {isLoading || !dashboard ? (
        <p className="text-sm text-slate-400">Carregando…</p>
      ) : (
        <>
          {research && <div className="mb-6 rounded-2xl border border-violet-100 bg-violet-50/70 p-5">
            <h2 className="font-bold text-slate-800">Resultados da pesquisa</h2>
            <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
              <StatCard icon={<StarIcon />} value={scaleScore(research.questionnaire.facilitator.susAverage, "/100")} label="SUS · facilitadores" />
              <StatCard icon={<StarIcon />} value={scaleScore(research.questionnaire.participants.susAverage, "/100")} label="SUS · convidados" />
              <StatCard icon={<StarIcon />} value={scaleScore(research.questionnaire.facilitator.uesAverage, "/5")} label="UES-SF · facilitadores" />
              <StatCard icon={<StarIcon />} value={scaleScore(research.questionnaire.participants.uesAverage, "/5")} label="UES-SF · convidados" />
              <StatCard icon={<UsersIcon />} value={scaleScore(research.questionnaire.facilitator.teamAverage, "/5")} label="Percepção da equipe · facilitadores" />
              <StatCard icon={<StarIcon />} value={`${research.questionnaire.customThemeUse.yes}/${research.questionnaire.customThemeUse.answered}`} label="Tema personalizado · Sim" />
              <StatCard icon={<UsersIcon />} value={`${research.receivedResponses}/${research.expectedResponses}`} label="Questionários respondidos" />
            </div>
            {research.facilitator.count + research.participants.count > 0 && <p className="mt-3 text-xs text-slate-500">
              {research.facilitator.count + research.participants.count} respostas do questionário anterior estão preservadas e não entram nas médias SUS e UES-SF.
            </p>}
            {research.questionnaire.count > 0 && <div className="mt-4 grid gap-3 md:grid-cols-2">
              <QuestionBreakdown title="Facilitadores" questions={research.questionnaire.facilitator.questions} facilitator />
              <QuestionBreakdown title="Convidados" questions={research.questionnaire.participants.questions} facilitator={false} />
            </div>}
            {research.suggestions.length > 0 && <div className="mt-4 border-t border-violet-100 pt-4">
              <h3 className="text-sm font-bold text-slate-800">Sugestões dos questionários</h3>
              <div className="mt-2 space-y-2">{research.suggestions.map((item, index) => <p key={index} className="rounded-xl bg-white px-3 py-2 text-sm text-slate-600">
                <span className="mr-2 font-semibold text-violet-700">{item.respondentRole === "FACILITATOR" ? "Facilitador" : "Convidado"}</span>{item.suggestion}</p>)}</div>
            </div>}
          </div>}
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <StatCard icon={<UsersIcon />} value={String(dashboard.averageParticipants)} label="Média de participantes" />
            <StatCard icon={<StarIcon />} value={scaleScore(research?.questionnaire.participants.uesAverage, "/5")} label="Engajamento dos convidados · UES-SF" />
            <StatCard icon={<ClockIcon />} value={`${dashboard.averageDurationMinutes} min`} label="Duração média" />
            <StatCard icon={<CalendarIcon />} value={String(dashboard.sessionsInPeriod)} label="Sessões no período" />
          </div>

          <div className="mb-6 grid grid-cols-1 gap-4 lg:grid-cols-3">
            <Card className="p-5">
              <h2 className="mb-4 flex items-center gap-2 text-sm font-bold text-slate-800">
                <ClockIcon width={16} height={16} /> Tempo médio por fase
              </h2>
              <div className="space-y-4">
                <BarRow
                  label="📝 Coleta"
                  value={dashboard.phaseAverages.collectMinutes}
                  max={dashboard.averageDurationMinutes}
                  valueLabel={phaseLabel(dashboard.phaseAverages.collectMinutes)}
                  colorClassName="bg-cyan-400"
                />
                <BarRow
                  label="🗳️ Votação"
                  value={dashboard.phaseAverages.voteMinutes}
                  max={dashboard.averageDurationMinutes}
                  valueLabel={phaseLabel(dashboard.phaseAverages.voteMinutes)}
                  colorClassName="bg-violet-400"
                />
                <BarRow
                  label="💬 Discussão"
                  value={dashboard.phaseAverages.discussMinutes}
                  max={dashboard.averageDurationMinutes}
                  valueLabel={phaseLabel(dashboard.phaseAverages.discussMinutes)}
                  colorClassName="bg-rose-400"
                />
              </div>
            </Card>

            <Card className="p-5">
              <h2 className="mb-4 text-sm font-bold text-slate-800">Modelos mais usados</h2>
              <div className="space-y-4">
                {dashboard.templatesUsage.map((usage) => (
                  <BarRow
                    key={usage.label}
                    label={usage.label}
                    value={usage.count}
                    max={dashboard.templatesUsage[0]?.count ?? 1}
                    valueLabel={`${usage.count}x`}
                    colorClassName="bg-cyan-400"
                  />
                ))}
              </div>
            </Card>

            <Card className="p-5">
              <h2 className="mb-4 text-sm font-bold text-slate-800">Temas mais escolhidos</h2>
              <div className="space-y-4">
                {dashboard.themesUsage.map((usage) => (
                  <BarRow
                    key={usage.label}
                    label={usage.label}
                    value={usage.count}
                    max={dashboard.themesUsage[0]?.count ?? 1}
                    valueLabel={`${usage.count}x`}
                    colorClassName="bg-violet-400"
                  />
                ))}
              </div>
            </Card>
          </div>

          <Card className="p-5">
            <h2 className="mb-4 text-sm font-bold text-slate-800">Participantes por sessão</h2>
            <ParticipantsLineChart series={dashboard.participantsSeries} />
          </Card>
        </>
      )}
    </div>
  );
}

function scaleScore(value: number | null | undefined, suffix: string) { return value == null ? "—" : `${value}${suffix}`; }

function QuestionBreakdown({ title, questions, facilitator }: { title: string;
  questions: { number: number; answered: number; average: number | null }[]; facilitator: boolean }) {
  const labels = facilitator ? [...susQuestions, ...uesQuestions, ...teamQuestions] : [...susQuestions, ...uesQuestions];
  return <details className="rounded-xl bg-white p-4 text-sm">
    <summary className="cursor-pointer font-bold text-violet-700">Média por afirmação · {title}</summary>
    <ol className="mt-3 space-y-2 text-slate-700">{questions.map(item => <li key={item.number}>
      {item.number}. {questionText(labels[item.number - 1], facilitator)} <strong>{scaleScore(item.average, "/5")}</strong>
    </li>)}</ol>
  </details>;
}
