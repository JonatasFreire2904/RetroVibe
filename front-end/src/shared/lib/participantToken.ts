const STORAGE_KEY = "retrovibe_participant_token";

export function getParticipantToken(): string | null {
  try { return localStorage.getItem(STORAGE_KEY); } catch { return null; }
}

export function setParticipantToken(token: string | null): void {
  try {
    if (token) localStorage.setItem(STORAGE_KEY, token);
    else localStorage.removeItem(STORAGE_KEY);
  } catch { /* Storage can be unavailable; staff authentication is independent. */ }
}
