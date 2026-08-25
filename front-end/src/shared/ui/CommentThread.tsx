import { useState } from "react";
import type { Comment } from "@/shared/types";

interface CommentThreadProps {
  comments: Comment[] | undefined;
  isLoading: boolean;
  onAdd: (text: string) => void;
  isAdding: boolean;
}

function formatTime(isoDate: string): string {
  const date = new Date(isoDate);
  return date.toLocaleString("pt-BR", { day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit" });
}

export function CommentThread({ comments, isLoading, onAdd, isAdding }: CommentThreadProps) {
  const [draft, setDraft] = useState("");

  function submit() {
    const text = draft.trim();
    if (!text) return;
    onAdd(text);
    setDraft("");
  }

  return (
    <div className="mt-2 space-y-2 border-t border-slate-100 pt-2" onClick={(e) => e.stopPropagation()}>
      {isLoading && <p className="text-xs text-slate-300">Carregando comentários…</p>}

      {!isLoading && comments && comments.length === 0 && (
        <p className="text-xs text-slate-300">Nenhum comentário ainda.</p>
      )}

      {!isLoading &&
        comments?.map((comment) => (
          <div key={comment.id} className="rounded-lg bg-slate-50 px-2.5 py-1.5 text-xs">
            <div className="flex items-center justify-between gap-2 text-slate-400">
              <span className="font-semibold text-slate-600">{comment.authorName ?? "Anônimo"}</span>
              <span className="font-mono">{formatTime(comment.createdAt)}</span>
            </div>
            <p className="mt-0.5 text-slate-700">{comment.text}</p>
          </div>
        ))}

      <div className="flex items-center gap-2">
        <input
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && submit()}
          placeholder="Comentar..."
          className="flex-1 rounded-lg border border-slate-200 px-2.5 py-1.5 text-xs outline-none placeholder:text-slate-400 focus:border-violet-400"
        />
        <button
          onClick={submit}
          disabled={isAdding || !draft.trim()}
          className="shrink-0 rounded-lg bg-violet-100 px-2.5 py-1.5 text-xs font-semibold text-violet-600 transition hover:bg-violet-200 disabled:opacity-40"
        >
          Enviar
        </button>
      </div>
    </div>
  );
}
