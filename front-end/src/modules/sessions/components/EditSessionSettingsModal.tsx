import { useEffect, useState } from "react";
import type { PrivacyMode, SessionBoard } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { Modal } from "@/shared/ui/Modal";
import { SessionSettingsFields } from "./SessionSettingsFields";

interface Props {
  board: SessionBoard;
  open: boolean;
  saving: boolean;
  onClose: () => void;
  onSave: (settings: { title: string | null; privacyMode: PrivacyMode; sequentialFlow: boolean; actionCardsEnabled: boolean; cardBlurEnabled: boolean }) => Promise<unknown>;
}

export function EditSessionSettingsModal({ board, open, saving, onClose, onSave }: Props) {
  const [title, setTitle] = useState(board.title ?? "");
  const [privacyMode, setPrivacyMode] = useState(board.privacyMode);
  const [sequentialFlow, setSequentialFlow] = useState(board.sequentialFlow);
  const [actionCardsEnabled, setActionCardsEnabled] = useState(board.actionCardsEnabled);
  const [cardBlurEnabled, setCardBlurEnabled] = useState(board.cardBlurEnabled);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!open) return;
    setTitle(board.title ?? "");
    setPrivacyMode(board.privacyMode);
    setSequentialFlow(board.sequentialFlow);
    setActionCardsEnabled(board.actionCardsEnabled);
    setCardBlurEnabled(board.cardBlurEnabled);
    setError(false);
  }, [open, board.id, board.title, board.privacyMode, board.sequentialFlow, board.actionCardsEnabled, board.cardBlurEnabled]);

  async function save() {
    setError(false);
    try {
      await onSave({ title: title.trim() || null, privacyMode, sequentialFlow, actionCardsEnabled, cardBlurEnabled });
      onClose();
    } catch {
      setError(true);
    }
  }

  return <Modal open={open} onClose={onClose} title="Configurações da sessão" subtitle="Atualize as regras enquanto a retrospectiva acontece"
    footer={<><Button variant="outline" onClick={onClose}>Cancelar</Button><Button disabled={saving} onClick={() => void save()}>{saving ? "Salvando…" : "Salvar alterações"}</Button></>}>
    <div className="mb-5">
      <label htmlFor="session-title" className="mb-2 block text-sm font-semibold text-slate-800">Nome da sessão</label>
      <input id="session-title" value={title} onChange={event => setTitle(event.target.value)} maxLength={100}
        placeholder="Ex.: Retrô Sprint 42" className="w-full rounded-xl border border-slate-200 px-4 py-3 text-sm text-slate-700 outline-none focus:border-violet-400" />
    </div>
    <SessionSettingsFields privacyMode={privacyMode} onPrivacyModeChange={setPrivacyMode}
      sequentialFlow={sequentialFlow} onSequentialFlowChange={setSequentialFlow}
      actionCardsEnabled={actionCardsEnabled} onActionCardsEnabledChange={setActionCardsEnabled}
      cardBlurEnabled={cardBlurEnabled} onCardBlurEnabledChange={setCardBlurEnabled} />
    {error && <p className="mt-3 text-sm text-rose-600">Não foi possível salvar. Tente novamente.</p>}
  </Modal>;
}
