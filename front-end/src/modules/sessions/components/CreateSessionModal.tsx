import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useThemesQuery } from "@/modules/catalog/api/queries";
import { useTemplatesQuery } from "@/modules/catalog/api/queries";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import type { PrivacyMode } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { Modal } from "@/shared/ui/Modal";
import { SearchIcon } from "@/shared/ui/Icons";
import { useCreateSessionMutation } from "../api/mutations";

interface CreateSessionModalProps {
  open: boolean;
  onClose: () => void;
  defaultSquadName?: string;
}

export function CreateSessionModal({ open, onClose, defaultSquadName }: CreateSessionModalProps) {
  const [step, setStep] = useState<1 | 2>(1);
  const [title, setTitle] = useState("");
  const [squadName, setSquadName] = useState(defaultSquadName ?? "");
  const [templateId, setTemplateId] = useState<string | null>(null);
  const [themeId, setThemeId] = useState<string | null>(null);
  const [privacyMode, setPrivacyMode] = useState<PrivacyMode>("IDENTIFIED");

  const { data: templates = [] } = useTemplatesQuery();
  const { data: themes = [] } = useThemesQuery();
  const { data: currentUser } = useCurrentUserQuery();
  const isAdmin = currentUser?.accessLevel === "ADMIN";
  const createSession = useCreateSessionMutation();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAdmin && currentUser?.squad) {
      setSquadName(currentUser.squad);
    }
  }, [isAdmin, currentUser?.squad]);

  function reset() {
    setStep(1);
    setTitle("");
    setSquadName(isAdmin ? defaultSquadName ?? "" : currentUser?.squad ?? "");
    setTemplateId(null);
    setThemeId(null);
    setPrivacyMode("IDENTIFIED");
    createSession.reset();
  }

  function handleClose() {
    reset();
    onClose();
  }

  async function handleCreate() {
    if (!templateId) return;
    const session = await createSession.mutateAsync({
      title: title.trim() || undefined,
      templateId,
      themeId: themeId ?? undefined,
      squadName: squadName.trim(),
      privacyMode,
    });
    reset();
    onClose();
    navigate(`/sessoes/${session.id}`);
  }

  const selectedTemplate = templates.find((t) => t.id === templateId);
  const selectedTheme = themes.find((t) => t.id === themeId);
  const canAdvance = squadName.trim().length > 0 && Boolean(templateId);

  return (
    <Modal
      open={open}
      onClose={handleClose}
      title="Nova sessão de retrospectiva"
      subtitle={step === 1 ? "Identifique o squad, o modelo e o tema" : "Confirme os detalhes da sessão"}
      footer={
        step === 1 ? (
          <>
            <Button variant="outline" onClick={handleClose}>
              Cancelar
            </Button>
            <Button disabled={!canAdvance} onClick={() => setStep(2)}>
              Próximo
            </Button>
          </>
        ) : (
          <>
            <Button variant="outline" onClick={() => setStep(1)}>
              Voltar
            </Button>
            <Button disabled={createSession.isPending} onClick={handleCreate}>
              {createSession.isPending ? "Criando…" : "Criar Sessão"}
            </Button>
          </>
        )
      }
    >
      <div className="flex justify-end pb-2">
        <div className="flex gap-1.5">
          <span className={`h-1.5 w-6 rounded-full ${step === 1 ? "bg-violet-500" : "bg-violet-200"}`} />
          <span className={`h-1.5 w-6 rounded-full ${step === 2 ? "bg-violet-500" : "bg-violet-200"}`} />
        </div>
      </div>

      {step === 1 ? (
        <div className="space-y-6">
          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-800">Título da sessão (opcional)</label>
            <input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Ex.: Retrô Sprint 42"
              maxLength={100}
              className="w-full rounded-xl border border-slate-200 bg-violet-50/40 px-4 py-3 text-sm text-slate-700 outline-none placeholder:text-slate-400 focus:border-violet-400"
            />
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-800">Squad</label>
            <div className="relative">
              <SearchIcon className="pointer-events-none absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
              <input
                value={squadName}
                onChange={(e) => setSquadName(e.target.value)}
                placeholder="Nome do squad..."
                disabled={!isAdmin}
                className="w-full rounded-xl border border-slate-200 bg-violet-50/40 py-3 pl-10 pr-4 text-sm text-slate-700 outline-none placeholder:text-slate-400 focus:border-violet-400 disabled:cursor-not-allowed disabled:opacity-70"
              />
            </div>
            {!isAdmin && <p className="mt-1.5 text-xs text-slate-400">Sessões são sempre criadas no seu squad.</p>}
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-800">Modelo de retrospectiva</label>
            <div className="grid grid-cols-2 gap-3">
              {templates.map((template) => (
                <button
                  key={template.id}
                  onClick={() => setTemplateId(template.id)}
                  className={`flex items-center gap-2 rounded-xl border px-4 py-3 text-left text-sm font-medium transition ${
                    templateId === template.id
                      ? "border-violet-400 bg-violet-50 text-violet-700"
                      : "border-slate-200 bg-violet-50/30 text-slate-600 hover:border-violet-200"
                  }`}
                >
                  <span>{template.icon}</span>
                  <span>{template.label}</span>
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-800">Tema da sessão</label>
            <div className="grid grid-cols-2 gap-3">
              {themes.map((theme) => (
                <button
                  key={theme.id}
                  onClick={() => setThemeId(theme.id)}
                  className={`flex items-center gap-2 rounded-xl border px-4 py-3 text-left text-sm font-medium transition ${
                    themeId === theme.id
                      ? "border-violet-400 bg-violet-50 text-violet-700"
                      : "border-slate-200 bg-violet-50/30 text-slate-600 hover:border-violet-200"
                  }`}
                >
                  <span>{theme.emoji}</span>
                  <span>{theme.label}</span>
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="mb-2 block text-sm font-semibold text-slate-800">Privacidade</label>
            <div className="grid grid-cols-2 gap-3">
              <button
                onClick={() => setPrivacyMode("IDENTIFIED")}
                className={`rounded-xl border px-4 py-3 text-left text-sm font-medium transition ${
                  privacyMode === "IDENTIFIED"
                    ? "border-violet-400 bg-violet-50 text-violet-700"
                    : "border-slate-200 bg-violet-50/30 text-slate-600 hover:border-violet-200"
                }`}
              >
                🙂 Identificado
                <p className="mt-0.5 text-xs font-normal text-slate-400">Todos veem quem escreveu cada card</p>
              </button>
              <button
                onClick={() => setPrivacyMode("ANONYMOUS")}
                className={`rounded-xl border px-4 py-3 text-left text-sm font-medium transition ${
                  privacyMode === "ANONYMOUS"
                    ? "border-violet-400 bg-violet-50 text-violet-700"
                    : "border-slate-200 bg-violet-50/30 text-slate-600 hover:border-violet-200"
                }`}
              >
                🕶️ Anônimo
                <p className="mt-0.5 text-xs font-normal text-slate-400">Autoria escondida de todo mundo</p>
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="space-y-3 rounded-xl border border-slate-100 bg-violet-50/30 p-5">
          {title.trim() && <SummaryRow label="Título" value={title.trim()} />}
          <SummaryRow label="Squad" value={squadName} />
          <SummaryRow label="Modelo" value={selectedTemplate ? `${selectedTemplate.icon} ${selectedTemplate.label}` : "—"} />
          <SummaryRow label="Tema" value={selectedTheme ? `${selectedTheme.emoji} ${selectedTheme.label}` : "Sem tema"} />
          <SummaryRow label="Privacidade" value={privacyMode === "ANONYMOUS" ? "🕶️ Anônimo" : "🙂 Identificado"} />
          {createSession.isError && (
            <p className="text-sm text-rose-600">Não foi possível criar a sessão. Tente novamente.</p>
          )}
        </div>
      )}
    </Modal>
  );
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between text-sm">
      <span className="text-slate-500">{label}</span>
      <span className="font-semibold text-slate-800">{value}</span>
    </div>
  );
}
