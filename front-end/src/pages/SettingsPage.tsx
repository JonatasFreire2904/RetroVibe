import { useState } from "react";
import { ProfileForm } from "@/modules/user/components/ProfileForm";

const TABS = ["Perfil", "Aparência", "Notificações", "Retrospectiva", "Segurança"] as const;

export function SettingsPage() {
  const [tab, setTab] = useState<(typeof TABS)[number]>("Perfil");

  return (
    <div className="mx-auto max-w-5xl">
      <h1 className="text-2xl font-extrabold text-slate-900">Configurações</h1>
      <p className="mt-1 text-sm text-slate-500">Personalize sua experiência RetroVibe</p>

      <div className="mt-6 grid grid-cols-1 gap-6 md:grid-cols-[220px_1fr]">
        <nav className="space-y-1">
          {TABS.map((item) => (
            <button
              key={item}
              onClick={() => setTab(item)}
              className={`block w-full rounded-xl px-3 py-2.5 text-left text-sm font-medium transition ${
                tab === item ? "bg-violet-50 text-violet-700" : "text-slate-500 hover:bg-slate-50"
              }`}
            >
              {item}
            </button>
          ))}
        </nav>

        <div>
          {tab === "Perfil" ? (
            <ProfileForm />
          ) : (
            <div className="flex h-40 items-center justify-center rounded-2xl border border-dashed border-slate-200 text-sm text-slate-400">
              Em breve.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
