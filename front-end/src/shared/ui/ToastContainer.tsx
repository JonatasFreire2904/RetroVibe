import { useEffect, useState } from "react";
import { dismissToast, subscribeToasts, Toast } from "@/shared/lib/toastStore";
import { XIcon } from "./Icons";

export function ToastContainer() {
  const [toasts, setToasts] = useState<Toast[]>([]);

  useEffect(() => subscribeToasts(setToasts), []);

  if (toasts.length === 0) return null;

  return (
    <div className="pointer-events-none fixed inset-x-0 top-4 z-[100] flex flex-col items-center gap-2 px-4">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={`pointer-events-auto flex max-w-md items-center gap-3 rounded-xl px-4 py-3 text-sm shadow-lg ${
            toast.tone === "error" ? "bg-rose-600 text-white" : "bg-slate-800 text-white"
          }`}
        >
          <span className="flex-1">{toast.message}</span>
          <button onClick={() => dismissToast(toast.id)} className="shrink-0 opacity-80 hover:opacity-100">
            <XIcon width={14} height={14} />
          </button>
        </div>
      ))}
    </div>
  );
}
