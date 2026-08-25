import { NavLink, useNavigate } from "react-router-dom";
import { useLogout } from "@/modules/auth/api/mutations";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { Avatar } from "@/shared/ui/Avatar";
import { CheckSquareIcon, ChartIcon, GearIcon, HistoryIcon, HomeIcon, LayersIcon, LogOutIcon } from "@/shared/ui/Icons";

const NAV_ITEMS = [
  { to: "/", label: "Home", icon: HomeIcon, end: true, adminOnly: false },
  { to: "/historico", label: "Histórico de Sessões", icon: HistoryIcon, adminOnly: false },
  { to: "/itens-de-acao", label: "Itens de Ação", icon: CheckSquareIcon, adminOnly: false },
  { to: "/dashboard", label: "Dashboard do Time", icon: ChartIcon, adminOnly: false },
  { to: "/admin/catalogo", label: "Catálogo", icon: LayersIcon, adminOnly: true },
  { to: "/configuracoes", label: "Configurações", icon: GearIcon, adminOnly: false },
];

export function Sidebar() {
  const { data: user } = useCurrentUserQuery();
  const logout = useLogout();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login", { replace: true });
  }

  return (
    <aside className="flex h-screen w-64 shrink-0 flex-col border-r border-slate-100 bg-white">
      <div className="flex items-center gap-2 px-6 py-6">
        <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-gradient text-lg">🔁</div>
        <div>
          <p className="font-display text-base font-extrabold leading-none text-slate-900">RetroVibe</p>
          <p className="mt-1 font-mono text-[10px] font-medium tracking-widest text-violet-400">
            RETROSPECTIVE PLATFORM
          </p>
        </div>
      </div>

      <nav className="flex-1 space-y-1 px-3">
        {NAV_ITEMS.filter((item) => !item.adminOnly || user?.accessLevel === "ADMIN").map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            end={item.end}
            className={({ isActive }) =>
              `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${
                isActive ? "bg-violet-50 text-violet-700" : "text-slate-500 hover:bg-slate-50 hover:text-slate-700"
              }`
            }
          >
            <item.icon className="shrink-0" />
            <span className="flex-1">{item.label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="border-t border-slate-100 px-4 py-4">
        <div className="flex items-center gap-3 rounded-xl px-2 py-2">
          <Avatar name={user?.name ?? "…"} color={user?.avatarColor ?? "#7C3AED"} />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm font-semibold text-slate-800">{user?.name ?? "Carregando…"}</p>
            <p className="truncate text-xs text-slate-400">
              {user?.role}
              {user?.squad ? ` · ${user.squad}` : ""}
            </p>
          </div>
          <button
            onClick={handleLogout}
            title="Sair"
            className="shrink-0 rounded-lg p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-rose-500"
          >
            <LogOutIcon width={16} height={16} />
          </button>
        </div>
      </div>
    </aside>
  );
}
