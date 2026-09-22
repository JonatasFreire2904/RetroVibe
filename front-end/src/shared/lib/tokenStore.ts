const STORAGE_KEY = "retrovibe_token";

type Listener = () => void;

let token: string | null = safeGet();
const listeners = new Set<Listener>();

function safeGet(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}

export function getToken(): string | null {
  return token;
}

export function setToken(next: string | null): void {
  token = next;
  try {
    if (next) localStorage.setItem(STORAGE_KEY, next);
    else localStorage.removeItem(STORAGE_KEY);
  } catch {
    // Mantém o token em memória quando o armazenamento local está indisponível.
  }
  listeners.forEach((listener) => listener());
}

export function subscribeToken(listener: Listener): () => void {
  listeners.add(listener);
  return () => listeners.delete(listener);
}
