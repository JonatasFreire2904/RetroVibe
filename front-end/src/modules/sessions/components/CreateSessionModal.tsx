import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSquadsQuery, useThemesQuery, useTemplatesQuery } from "@/modules/catalog/api/queries";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import type { PrivacyMode } from "@/shared/types";
import { Button } from "@/shared/ui/Button";
import { Modal } from "@/shared/ui/Modal";
import { SearchIcon } from "@/shared/ui/Icons";
import { useCreateSessionMutation } from "../api/mutations";
import { getRetroTheme } from "../retroTheme";
import { SessionSettingsFields } from "./SessionSettingsFields";

interface CreateSessionModalProps {
  open: boolean;
  onClose: () => void;
}

export function CreateSessionModal({ open, onClose }: CreateSessionModalProps) {
  const [step, setStep] = useState<1 | 2>(1);
  const [title, setTitle] = useState("");
  const [squadName, setSquadName] = useState("");
  const [squadListOpen, setSquadListOpen] = useState(false);
  const [squadSelected, setSquadSelected] = useState(false);
  const [templateId, setTemplateId] = useState<string | null>(null);
  const [themeId, setThemeId] = useState<string | null>(null);
  const [privacyMode, setPrivacyMode] = useState<PrivacyMode>("IDENTIFIED");
  const [sequentialFlow, setSequentialFlow] = useState(true);
  const [actionCardsEnabled, setActionCardsEnabled] = useState(true);

  const { data: templates = [] } = useTemplatesQuery();
  const { data: themes = [] } = useThemesQuery();
  const { data: squads = [] } = useSquadsQuery();
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
    setSquadName(isAdmin ? "" : currentUser?.squad ?? "");
    setSquadListOpen(false);
    setSquadSelected(false);
    setTemplateId(null);
    setThemeId(null);
    setPrivacyMode("IDENTIFIED");
    setSequentialFlow(true);
    setActionCardsEnabled(true);
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
      sequentialFlow,
      actionCardsEnabled,
    });
    reset();
    onClose();
    navigate(`/sessoes/${session.id}`);
  }

  const matchingSquads = squads.filter(squad =>
    squadSelected || squad.name.toLocaleLowerCase("pt-BR").includes(squadName.trim().toLocaleLowerCase("pt-BR")));
  const existingSquad = squads.some(squad => squad.name.toLocaleLowerCase("pt-BR") === squadName.trim().toLocaleLowerCase("pt-BR"));
  const canAdvance = squadName.trim().length > 0 && Boolean(templateId) && (!isAdmin || squadSelected || existingSquad);

  function selectSquad(name: string) {
    setSquadName(name);
    setSquadSelected(true);
    setSquadListOpen(false);
  }

  return (
    <Modal
      open={open}
      onClose={handleClose}
      title={step === 1 ? "Nova sessão de retrospectiva" : "Configurações da sessão"}
      subtitle={step === 1 ? "Identifique o squad, o modelo e o tema" : "Defina as regras de comportamento da sessão"}
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
                onChange={(e) => { setSquadName(e.target.value); setSquadSelected(false); setSquadListOpen(true); }}
                onFocus={() => isAdmin && setSquadListOpen(true)}
                onKeyDown={(e) => { if (e.key === "Escape") setSquadListOpen(false); }}
                placeholder={isAdmin ? "Selecione ou busque um squad..." : "Nome do squad..."}
                role={isAdmin ? "combobox" : undefined}
                aria-expanded={isAdmin ? squadListOpen : undefined}
                aria-controls={isAdmin ? "squad-options" : undefined}
                aria-autocomplete={isAdmin ? "list" : undefined}
                disabled={!isAdmin}
                className="w-full rounded-xl border border-slate-200 bg-violet-50/40 py-3 pl-10 pr-10 text-sm text-slate-700 outline-none placeholder:text-slate-400 focus:border-violet-400 disabled:cursor-not-allowed disabled:opacity-70"
              />
              {isAdmin && <button type="button" aria-label="Mostrar squads" aria-expanded={squadListOpen}
                onClick={() => setSquadListOpen(value => !value)}
                className="absolute right-2 top-1/2 flex h-8 w-8 -translate-y-1/2 items-center justify-center rounded-lg text-slate-500 hover:bg-violet-100">
                <span className={`text-lg transition-transform ${squadListOpen ? "rotate-180" : ""}`}>⌄</span>
              </button>}
              {isAdmin && squadListOpen && <div id="squad-options" role="listbox" aria-label="Squads disponíveis"
                className="absolute left-0 right-0 top-full z-20 mt-2 max-h-44 overflow-y-auto rounded-xl border border-violet-200 bg-white p-1.5 shadow-lg">
              {matchingSquads.map(squad => <button key={squad.id} type="button" role="option"
                aria-selected={squad.name === squadName} onClick={() => selectSquad(squad.name)}
                className="block w-full rounded-lg px-3 py-2 text-left text-sm text-slate-700 hover:bg-violet-50 focus:bg-violet-50 focus:outline-none">
                {squad.name}
              </button>)}
              {matchingSquads.length === 0 && <p className="px-3 py-2 text-sm text-slate-500">Nenhum squad encontrado.</p>}
              {squadName.trim() && !existingSquad && <button type="button" role="option" aria-selected={false}
                onClick={() => selectSquad(squadName.trim())}
                className="block w-full rounded-lg px-3 py-2 text-left text-sm font-semibold text-violet-700 hover:bg-violet-50 focus:bg-violet-50 focus:outline-none">
                Criar novo squad “{squadName.trim()}”
              </button>}
              </div>}
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
                  <span>{getRetroTheme(theme.key).icon}</span>
                  <span>{theme.label}</span>
                </button>
              ))}
            </div>
          </div>

        </div>
      ) : (
        <div>
          <SessionSettingsFields privacyMode={privacyMode} onPrivacyModeChange={setPrivacyMode}
            sequentialFlow={sequentialFlow} onSequentialFlowChange={setSequentialFlow}
            actionCardsEnabled={actionCardsEnabled} onActionCardsEnabledChange={setActionCardsEnabled} />
          {createSession.isError && <p className="mt-3 text-sm text-rose-600">Não foi possível criar a sessão. Tente novamente.</p>}
        </div>
      )}
    </Modal>
  );
}
