import { useState } from "react";
import { useHomeDataQuery } from "@/modules/sessions/api/queries";
import { CreateSessionModal } from "@/modules/sessions/components/CreateSessionModal";
import { QuickTemplateCard } from "@/modules/sessions/components/QuickTemplateCard";
import { RecentSessionCard } from "@/modules/sessions/components/RecentSessionCard";
import { Badge } from "@/shared/ui/Badge";
import { Button } from "@/shared/ui/Button";
import { PlusIcon, RefreshIcon } from "@/shared/ui/Icons";

export function HomePage() {
  const [modalOpen, setModalOpen] = useState(false);
  const { data, isLoading } = useHomeDataQuery();

  return (
    <div className="mx-auto max-w-5xl">
      <section className="rounded-3xl bg-gradient-to-b from-violet-50 to-transparent px-6 py-14 text-center">
        <Badge variant="violet" className="bg-white shadow-sm">
          ✦ RETROSPECTIVAS ÁGEIS TEMÁTICAS
        </Badge>
        <h1 className="mx-auto mt-6 max-w-2xl text-4xl font-extrabold leading-tight text-slate-900">
          Reflita. Melhore.
          <br />
          <span className="bg-brand-gradient bg-clip-text text-transparent">Evolua juntos.</span>
        </h1>
        <p className="mx-auto mt-4 max-w-xl text-sm text-slate-500">
          Crie sessões de retrospectiva envolventes com formatos temáticos, colaboração em tempo real e dinâmicas que
          engajam o time.
        </p>
        <Button className="mx-auto mt-8" icon={<PlusIcon />} onClick={() => setModalOpen(true)}>
          Criar Sessão de Retrospectiva
        </Button>
      </section>

      <section className="mt-6">
        <div className="mb-4 flex items-center gap-2 border-b border-slate-100 pb-3">
          <RefreshIcon className="text-slate-400" />
          <h2 className="text-sm font-bold text-slate-800">Últimos modelos usados</h2>
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          {isLoading && <p className="text-sm text-slate-400">Carregando…</p>}
          {data?.recentSessions.length === 0 && (
            <p className="text-sm text-slate-400">Nenhuma sessão ainda. Crie a primeira retrospectiva do time!</p>
          )}
          {data?.recentSessions.map((session) => (
            <RecentSessionCard key={session.id} session={session} />
          ))}
        </div>
      </section>

      <section className="mt-10">
        <div className="mb-4 flex items-center justify-between border-b border-slate-100 pb-3">
          <h2 className="text-sm font-bold text-slate-800">Modelos Rápidos</h2>
          <span className="text-xs text-slate-400">Inicie com 1 clique</span>
        </div>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          {data?.quickTemplates.map((template) => (
            <QuickTemplateCard key={template.id} template={template} />
          ))}
        </div>
      </section>

      <CreateSessionModal open={modalOpen} onClose={() => setModalOpen(false)} />
    </div>
  );
}
