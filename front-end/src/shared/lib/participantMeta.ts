const STORAGE_KEY = "retrovibe_participant";

export interface ParticipantMeta {
  id: string;
  name: string;
  sessionId: string;
}

export function setParticipantMeta(meta: ParticipantMeta): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(meta));
  } catch {
    // O navegador pode bloquear o armazenamento local.
  }
}

export function getParticipantMeta(): ParticipantMeta | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as ParticipantMeta) : null;
  } catch {
    return null;
  }
}

export function clearParticipantMeta(): void {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // O navegador pode bloquear o armazenamento local.
  }
}
