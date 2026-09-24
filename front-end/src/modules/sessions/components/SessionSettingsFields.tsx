import type { PrivacyMode } from "@/shared/types";

interface Props {
  privacyMode: PrivacyMode;
  onPrivacyModeChange: (value: PrivacyMode) => void;
  sequentialFlow: boolean;
  onSequentialFlowChange: (value: boolean) => void;
  actionCardsEnabled: boolean;
  onActionCardsEnabledChange: (value: boolean) => void;
  cardBlurEnabled: boolean;
  onCardBlurEnabledChange: (value: boolean) => void;
}

export function SessionSettingsFields({ privacyMode, onPrivacyModeChange, sequentialFlow, onSequentialFlowChange,
  actionCardsEnabled, onActionCardsEnabledChange, cardBlurEnabled, onCardBlurEnabledChange }: Props) {
  return <div className="space-y-5">
    <fieldset>
      <legend className="mb-2 text-sm font-semibold text-slate-800">Privacidade</legend>
      <div className="grid grid-cols-2 gap-3">
        {([
          { value: "IDENTIFIED", icon: "🙂", label: "Identificado", help: "Todos veem quem escreveu cada card" },
          { value: "ANONYMOUS", icon: "🕶️", label: "Anônimo", help: "Autoria escondida de todo mundo" },
        ] as const).map(option => <button key={option.value} type="button" onClick={() => onPrivacyModeChange(option.value)}
          aria-pressed={privacyMode === option.value}
          className={`rounded-xl border px-4 py-3 text-left text-sm font-medium transition ${privacyMode === option.value
            ? "border-violet-400 bg-violet-50 text-violet-700" : "border-slate-200 bg-violet-50/30 text-slate-600 hover:border-violet-200"}`}>
          {option.icon} {option.label}
          <span className="mt-0.5 block text-xs font-normal text-slate-400">{option.help}</span>
        </button>)}
      </div>
    </fieldset>
    <SettingSwitch value={sequentialFlow} onChange={onSequentialFlowChange} title="Fluxo sequencial de etapas"
      description="Os participantes concluem cada coluna antes de passar para a próxima. Desative para preencher todas ao mesmo tempo." />
    <SettingSwitch value={actionCardsEnabled} onChange={onActionCardsEnabledChange} title="Coluna de Itens de Ação"
      description="Uma coluna extra de Itens de Ação aparece ao final do quadro para registrar compromissos da equipe." />
    <SettingSwitch value={cardBlurEnabled} onChange={onCardBlurEnabledChange} title="Card Blur"
      description="Cards criados por outras pessoas ficam borrados até o facilitador liberar a visualização. Evita ancoragem de opiniões." />
  </div>;
}

function SettingSwitch({ value, onChange, title, description }: { value: boolean; onChange: (value: boolean) => void; title: string; description: string }) {
  return <button type="button" role="switch" aria-checked={value} onClick={() => onChange(!value)}
    className="flex w-full items-center justify-between gap-4 rounded-2xl border border-violet-100 bg-violet-50/60 p-4 text-left">
    <span><span className="block text-sm font-bold text-slate-800">{title}</span>
      <span className="mt-1 block text-xs leading-relaxed text-slate-500">{description}</span></span>
    <span className={`relative h-6 w-11 shrink-0 rounded-full transition ${value ? "bg-violet-500" : "bg-slate-300"}`}>
      <span className={`absolute top-1 h-4 w-4 rounded-full bg-white transition ${value ? "left-6" : "left-1"}`} />
    </span>
  </button>;
}
