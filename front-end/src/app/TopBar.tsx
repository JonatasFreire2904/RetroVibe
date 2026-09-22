import { useLocation } from "react-router-dom";
import { BellIcon } from "@/shared/ui/Icons";

const SECTIONS: { prefix: string; title: string; subtitle: string }[] = [
  { prefix: "/historico", title: "Histórico de Sessões", subtitle: "Todas as retrospectivas realizadas pela equipe" },
  { prefix: "/sessoes", title: "Sessão de Retrospectiva", subtitle: "Colabore com o time em tempo real" },
  { prefix: "/itens-de-acao", title: "Itens de Ação", subtitle: "Acompanhe compromissos e responsáveis por sessão" },
  { prefix: "/dashboard", title: "Dashboard do Time", subtitle: "Métricas e análises agregadas das sessões" },
  { prefix: "/configuracoes", title: "Configurações", subtitle: "Preferências da plataforma" },
  { prefix: "/", title: "Home", subtitle: "Crie ou retome uma sessão de retrospectiva" },
];

export function TopBar() {
  const { pathname } = useLocation();
  const section = SECTIONS.find((s) => (s.prefix === "/" ? pathname === "/" : pathname.startsWith(s.prefix)));

  return (
    <header className="flex h-[73px] shrink-0 items-center justify-between border-b border-slate-100 bg-white px-8">
      <div>
        <h1 className="text-base font-bold text-slate-900">{section?.title}</h1>
        <p className="text-xs text-slate-400">{section?.subtitle}</p>
      </div>
      <button className="relative rounded-full bg-slate-50 p-2.5 text-slate-500 transition hover:bg-slate-100">
        <BellIcon />
        <span className="absolute right-2 top-2 h-1.5 w-1.5 rounded-full bg-fuchsia-500" />
      </button>
    </header>
  );
}
