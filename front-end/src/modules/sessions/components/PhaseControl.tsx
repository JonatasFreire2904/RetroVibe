import type { SessionPhase } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { ArrowRightIcon } from "@/shared/ui/Icons";

const PHASES: { value: SessionPhase; label: string; icon: string }[] = [
  { value: "COLLECTING", label: "Coleta", icon: "📝" },
  { value: "VOTING", label: "Votação", icon: "🗳️" },
  { value: "DISCUSSING", label: "Discussão", icon: "💬" },
];

interface PhaseControlProps {
  phase: SessionPhase;
  canControl: boolean;
  onAdvance: () => void;
  isAdvancing: boolean;
}

export function PhaseControl({ phase, canControl, onAdvance, isAdvancing }: PhaseControlProps) {
  const currentIndex = PHASES.findIndex((p) => p.value === phase);
  const isLastPhase = currentIndex === PHASES.length - 1;

  return (
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
      <div className="flex items-center gap-2">
        {PHASES.map((p, index) => (
          <div key={p.value} className="flex items-center gap-2">
            <span
              className={`flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-semibold ${
                index === currentIndex
                  ? "bg-violet-600 text-white"
                  : index < currentIndex
                    ? "bg-violet-100 text-violet-500"
                    : "bg-slate-100 text-slate-400"
              }`}
            >
              {p.icon} {p.label}
            </span>
            {index < PHASES.length - 1 && <ArrowRightIcon width={12} height={12} className="text-slate-300" />}
          </div>
        ))}
      </div>

      {canControl && !isLastPhase && (
        <Button size="sm" onClick={onAdvance} disabled={isAdvancing}>
          {isAdvancing ? "Avançando…" : `Avançar para ${PHASES[currentIndex + 1]!.label}`}
        </Button>
      )}
    </div>
  );
}
