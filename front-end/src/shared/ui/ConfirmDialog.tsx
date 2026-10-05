import { Button } from "./Button";
import { Modal } from "./Modal";

interface ConfirmDialogProps {
  open: boolean;
  title: string;
  message: string;
  confirmLabel: string;
  pendingLabel?: string;
  tone?: "primary" | "danger";
  isPending?: boolean;
  error?: string | null;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  open, title, message, confirmLabel, pendingLabel, tone = "primary", isPending = false, error, onConfirm, onCancel,
}: ConfirmDialogProps) {
  const cancel = () => { if (!isPending) onCancel(); };
  return (
    <Modal
      open={open}
      onClose={cancel}
      title={title}
      size="sm"
      footer={
        <div className="ml-auto flex gap-2">
          <Button variant="outline" size="sm" onClick={cancel} disabled={isPending}>Cancelar</Button>
          <Button variant={tone} size="sm" onClick={onConfirm} disabled={isPending} autoFocus>
            {isPending ? pendingLabel ?? confirmLabel : confirmLabel}
          </Button>
        </div>
      }
    >
      <p className="text-sm text-slate-600">{message}</p>
      {error && <p className="mt-3 text-sm text-rose-600">{error}</p>}
    </Modal>
  );
}
