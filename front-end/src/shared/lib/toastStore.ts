export interface Toast {
  id: string;
  message: string;
  tone: "error" | "info";
}

type Listener = (toasts: Toast[]) => void;

let toasts: Toast[] = [];
const listeners = new Set<Listener>();

function emit(): void {
  listeners.forEach((listener) => listener(toasts));
}

export function pushToast(message: string, tone: Toast["tone"] = "error"): void {
  const toast: Toast = { id: crypto.randomUUID(), message, tone };
  toasts = [...toasts, toast];
  emit();
  setTimeout(() => dismissToast(toast.id), 5000);
}

export function dismissToast(id: string): void {
  toasts = toasts.filter((t) => t.id !== id);
  emit();
}

export function subscribeToasts(listener: Listener): () => void {
  listeners.add(listener);
  listener(toasts);
  return () => listeners.delete(listener);
}
