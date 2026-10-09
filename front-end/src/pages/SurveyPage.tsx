import { useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Link, Navigate, useNavigate, useParams } from 'react-router-dom';
import { httpClient, ApiError } from '@/shared/lib/httpClient';
import { getParticipantMeta } from '@/shared/lib/participantMeta';
import { getParticipantToken } from '@/shared/lib/participantToken';
import { questionText, susQuestions, teamQuestions, uesQuestions, type SurveyQuestion } from '@/shared/surveyQuestionnaire';

interface SurveyStatus {
  enabled: boolean; open: boolean; completed: boolean;
  role: 'FACILITATOR' | 'PARTICIPANT'; sessionDate: string;
}

const scale = ['Discordo totalmente', 'Discordo parcialmente', 'Neutro', 'Concordo parcialmente', 'Concordo totalmente'];

export function SurveyPage({ guest = false }: { guest?: boolean }) {
  const { sessionId } = useParams<{ sessionId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [ratings, setRatings] = useState<Record<number, number>>({});
  const [usedCustomTheme, setUsedCustomTheme] = useState<boolean | null>(null);
  const [customThemeName, setCustomThemeName] = useState('');
  const [suggestion, setSuggestion] = useState('');
  const meta = guest ? getParticipantMeta() : null;
  const hasGuestAccess = !guest || (meta?.sessionId === sessionId && Boolean(getParticipantToken()));
  const status = useQuery({ queryKey: ['survey', sessionId, guest],
    queryFn: () => httpClient.get<SurveyStatus>(`/sessions/${sessionId}/survey`),
    enabled: Boolean(sessionId && hasGuestAccess), refetchOnWindowFocus: false });
  const result = status.data;
  const isFacilitator = result?.role === 'FACILITATOR';
  const questions = isFacilitator ? [...susQuestions, ...uesQuestions, ...teamQuestions] : [...susQuestions, ...uesQuestions];
  const answered = questions.filter(question => ratings[question.number] !== undefined).length;
  const canSubmit = Boolean(result?.open && answered === questions.length && usedCustomTheme !== null &&
    (!isFacilitator || !usedCustomTheme || customThemeName.trim()) && suggestion.trim());
  const submit = useMutation({ mutationFn: () => httpClient.post(`/sessions/${sessionId}/survey`, {
    usedCustomTheme, customThemeName: isFacilitator && usedCustomTheme ? customThemeName.trim() : null,
    ratings: questions.map(question => ratings[question.number]), suggestion: suggestion.trim() }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['survey', sessionId] });
      queryClient.invalidateQueries({ queryKey: ['pending-surveys'] });
      if (!guest) navigate('/', { replace: true }); }
  });
  if (!hasGuestAccess) return <Navigate to={`/entrar/${sessionId ?? ''}`} replace />;
  const completed = result?.completed || submit.isSuccess;
  function handleSubmit(event: FormEvent) { event.preventDefault(); if (canSubmit) submit.mutate(); }
  function setRating(number: number, score: number) { setRatings(previous => ({ ...previous, [number]: score })); }

  return <div className='min-h-screen bg-gradient-to-b from-violet-100 via-white to-rose-50 px-4 py-10'>
    <div className='mx-auto max-w-3xl rounded-3xl border border-violet-100 bg-white p-6 shadow-xl sm:p-8'>
      <p className='text-xs font-bold uppercase tracking-widest text-violet-500'>RetroVibe · Pesquisa</p>
      <h1 className='mt-2 text-2xl font-extrabold text-slate-900'>Avaliação de usabilidade e engajamento</h1>
      <p className='mt-2 text-sm text-slate-600'>Este questionário leva cerca de 5 minutos. Pense na retrospectiva que você acabou de realizar. Não há respostas certas ou erradas.</p>
      {result && <p className='mt-2 text-sm text-slate-500'>Data da retrospectiva: {new Date(result.sessionDate).toLocaleDateString('pt-BR')}</p>}
      {guest && <p className='mt-3 rounded-xl bg-violet-50 p-3 text-sm text-violet-800'>Suas respostas são compartilhadas de forma anônima com o criador da sessão.</p>}
      {status.isLoading && <p className='mt-6 text-sm text-slate-500'>Carregando questionário…</p>}
      {status.isError && <p className='mt-6 text-sm text-rose-600'>Não foi possível abrir o questionário. Atualize a página e tente novamente.</p>}
      {completed && <div className='mt-7 rounded-2xl bg-emerald-50 p-5 text-emerald-800'><strong>Obrigado por responder!</strong>
        <p className='mt-1 text-sm'>Sua avaliação foi registrada.</p>{!guest && <Link to='/' className='mt-3 inline-block font-semibold underline'>Voltar à Home</Link>}</div>}
      {result && !result.enabled && !completed && <p className='mt-6 text-sm text-slate-600'>Esta sessão não tem questionário de avaliação.</p>}
      {result?.enabled && !result.open && !completed && <p className='mt-6 text-sm text-slate-600'>O questionário será liberado quando o facilitador encerrar a sessão.</p>}
      {result?.open && !completed && <form className='mt-7 space-y-8' onSubmit={handleSubmit}>
        <div className='rounded-2xl bg-slate-50 p-4 text-sm text-slate-700'>
          <p className='font-semibold'>Como responder</p>
          <p className='mt-1'>Marque uma opção de 1 a 5 para cada afirmação. Todas as afirmações e a pergunta aberta são obrigatórias.</p>
          <div className='mt-3 grid gap-1 sm:grid-cols-5'>{scale.map((label, index) => <span key={label}><strong>{index + 1}</strong> · {label}</span>)}</div>
        </div>
        <section className='rounded-2xl border border-violet-100 p-5'>
          <h2 className='text-lg font-bold text-slate-900'>Dados da sessão</h2>
          <YesNoQuestion label={isFacilitator
            ? 'Você utilizou algum tema personalizado (Natal, Páscoa etc.) nesta sessão?'
            : 'A sessão utilizou um tema personalizado (Natal, Páscoa etc.)?'}
            value={usedCustomTheme} onChange={setUsedCustomTheme} />
          {isFacilitator && usedCustomTheme && <label className='mt-4 block text-sm font-semibold text-slate-800'>
            Se sim, qual tema? <span className='text-rose-500'>*</span>
            <input value={customThemeName} onChange={event => setCustomThemeName(event.target.value)}
              maxLength={120} required className='mt-2 w-full rounded-xl border border-slate-200 p-3 outline-none focus:border-violet-400' />
          </label>}
        </section>
        <QuestionSection title='A. Usabilidade · SUS' questions={susQuestions} ratings={ratings}
          facilitator={isFacilitator} onChange={setRating} />
        <QuestionSection title='B. Engajamento · UES-SF' questions={uesQuestions} ratings={ratings}
          facilitator={isFacilitator} onChange={setRating}
          description={isFacilitator ? 'Responda sobre a sua própria experiência.' : undefined} />
        {isFacilitator && <QuestionSection title='C. Itens complementares sobre a equipe' questions={teamQuestions}
          ratings={ratings} facilitator onChange={setRating} />}
        <section className='rounded-2xl border border-violet-100 p-5'>
          <h2 className='text-lg font-bold text-slate-900'>Pergunta aberta</h2>
          <label className='mt-3 block text-sm font-semibold text-slate-800'>
            Qual sugestão de melhoria você apresenta para o tema ou layout utilizado nesta cerimônia? <span className='text-rose-500'>*</span>
            <textarea value={suggestion} onChange={event => setSuggestion(event.target.value)} maxLength={2000} rows={4}
              required placeholder='Se não tiver sugestões, escreva Nenhuma.'
              className='mt-2 w-full rounded-xl border border-slate-200 p-3 font-normal outline-none focus:border-violet-400' />
          </label>
          {guest && <p className='mt-2 text-xs text-slate-500'>Essa informação será compartilhada de forma anônima com o criador da sessão.</p>}
        </section>
        {submit.isError && <p className='text-sm text-rose-600'>{submit.error instanceof ApiError ? submit.error.message : 'Não foi possível enviar. Tente novamente.'}</p>}
        <div className='flex flex-wrap items-center justify-between gap-3'>
          <p className='text-sm text-slate-500'>{answered} de {questions.length} afirmações respondidas</p>
          <button disabled={!canSubmit || submit.isPending}
            className='rounded-xl bg-violet-600 px-6 py-3 font-bold text-white disabled:opacity-50'>
            {submit.isPending ? 'Enviando…' : 'Enviar avaliação'}
          </button>
        </div>
      </form>}
    </div>
  </div>;
}

function QuestionSection({ title, description, questions, ratings, facilitator, onChange }: {
  title: string; description?: string; questions: SurveyQuestion[]; ratings: Record<number, number>;
  facilitator: boolean; onChange: (number: number, score: number) => void;
}) {
  return <section className='rounded-2xl border border-violet-100 p-5'>
    <h2 className='text-lg font-bold text-slate-900'>{title}</h2>
    {description && <p className='mt-1 text-sm text-slate-500'>{description}</p>}
    <div className='mt-4 divide-y divide-slate-100'>{questions.map(question => <fieldset key={question.number} className='py-4 first:pt-0 last:pb-0'>
      <legend className='text-sm font-semibold text-slate-800'>{question.number}. {questionText(question, facilitator)} <span className='text-rose-500'>*</span></legend>
      <div className='mt-3 flex flex-wrap gap-2'>{scale.map((label, index) => {
        const score = index + 1;
        return <label key={score} className={'cursor-pointer rounded-xl border px-3 py-2 text-sm font-semibold transition ' +
          (ratings[question.number] === score ? 'border-violet-500 bg-violet-100 text-violet-800' : 'border-slate-200 text-slate-600 hover:border-violet-300')}>
          <input type='radio' name={`rating-${question.number}`} value={score}
            checked={ratings[question.number] === score} onChange={() => onChange(question.number, score)}
            required className='sr-only' aria-label={`${score} — ${label}`} />{score}
        </label>;
      })}</div>
    </fieldset>)}</div>
  </section>;
}

function YesNoQuestion({ label, value, onChange }: { label: string; value: boolean | null; onChange: (value: boolean) => void }) {
  return <fieldset className='mt-4'><legend className='text-sm font-semibold text-slate-800'>{label} <span className='text-rose-500'>*</span></legend>
    <div className='mt-3 flex gap-2'>{[true, false].map(answer => <label key={String(answer)}
      className={'cursor-pointer rounded-xl border px-5 py-2 text-sm font-semibold ' + (value === answer
        ? 'border-violet-500 bg-violet-100 text-violet-800' : 'border-slate-200 text-slate-600 hover:border-violet-300')}>
      <input type='radio' name='used-custom-theme' checked={value === answer} onChange={() => onChange(answer)}
        required className='sr-only' />{answer ? 'Sim' : 'Não'}</label>)}</div>
  </fieldset>;
}
