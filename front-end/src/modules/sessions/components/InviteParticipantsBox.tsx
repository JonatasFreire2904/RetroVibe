import { useState } from "react";
import { UsersIcon } from "@/shared/ui/Icons";

export function InviteParticipantsBox({ sessionId }: { sessionId: string }) {
  const [copied, setCopied] = useState(false);
  const link = `${window.location.origin}/entrar/${sessionId}`;

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(link);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
    }
  }

  return (
    <div className="mb-6 flex flex-wrap items-center gap-3 rounded-2xl border border-dashed border-violet-200 bg-violet-50/50 p-4">
      <UsersIcon className="shrink-0 text-violet-500" />
      <div className="min-w-[220px] flex-1">
        <p className="text-sm font-semibold text-violet-800">Convidar participantes</p>
        <p className="text-xs text-violet-500">Compartilhe este link — quem entrar não precisa de conta.</p>
      </div>
      <input
        readOnly
        value={link}
        onFocus={(e) => e.currentTarget.select()}
        className="min-w-0 flex-1 rounded-lg border border-violet-200 bg-white px-3 py-2 text-xs text-slate-600 outline-none"
      />
      <button
        onClick={handleCopy}
        className="shrink-0 rounded-lg bg-violet-600 px-3 py-2 text-xs font-semibold text-white transition hover:bg-violet-700"
      >
        {copied ? "Copiado!" : "Copiar link"}
      </button>
    </div>
  );
}
