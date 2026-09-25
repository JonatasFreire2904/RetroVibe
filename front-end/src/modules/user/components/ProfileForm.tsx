import { useEffect, useState } from "react";
import { useCurrentUserQuery } from "@/modules/user/api/queries";
import { useUpdateProfileMutation } from "@/modules/user/api/mutations";
import { Avatar } from "@/shared/ui/Avatar";
import { Button } from "@/shared/ui/Button";
import { CheckIcon } from "@/shared/ui/Icons";

const AVATAR_COLORS = ["#7C3AED", "#06B6D4", "#EC4899", "#22C55E", "#F97316", "#6366F1", "#DB2777", "#14B8A6"];

export function ProfileForm() {
  const { data: user } = useCurrentUserQuery();
  const updateProfile = useUpdateProfileMutation();

  const [name, setName] = useState("");
  const [role, setRole] = useState("");
  const [avatarColor, setAvatarColor] = useState(AVATAR_COLORS[0]);

  useEffect(() => {
    if (!user) return;
    setName(user.name);
    setRole(user.role);
    setAvatarColor(user.avatarColor);
  }, [user]);

  function handleSubmit() {
    updateProfile.mutate({ name, role, avatarColor });
  }

  return (
    <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
      <h2 className="mb-4 flex items-center gap-2 text-sm font-bold text-slate-800">Perfil</h2>
      <div className="border-b border-slate-100 pb-6">
        <div className="flex items-center gap-4">
          <Avatar name={name || "?"} color={avatarColor} />
          <div>
            <p className="mb-2 text-xs font-medium text-slate-500">Cor do avatar</p>
            <div className="flex gap-2">
              {AVATAR_COLORS.map((color) => (
                <button
                  key={color}
                  onClick={() => setAvatarColor(color)}
                  className="flex h-7 w-7 items-center justify-center rounded-full transition"
                  style={{ backgroundColor: color, boxShadow: color === avatarColor ? `0 0 0 2px white, 0 0 0 4px ${color}` : undefined }}
                >
                  {color === avatarColor && <CheckIcon width={14} height={14} className="text-white" />}
                </button>
              ))}
            </div>
          </div>
        </div>
      </div>

      <div className="pt-6">
        <p className="mb-3 text-xs font-bold uppercase tracking-wide text-slate-400">Informações pessoais</p>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field label="Nome" value={name} onChange={setName} />
          <Field label="Cargo" value={role} onChange={setRole} />
        </div>

        <Button className="mt-6" icon={<CheckIcon width={16} height={16} />} onClick={handleSubmit} disabled={updateProfile.isPending}>
          {updateProfile.isPending ? "Salvando…" : "Salvar alterações"}
        </Button>
        {updateProfile.isSuccess && <span className="ml-3 text-sm text-emerald-600">Perfil atualizado!</span>}
      </div>
    </div>
  );
}

function Field({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm text-slate-500">{label}</span>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3.5 py-2.5 text-sm text-slate-700 outline-none focus:border-violet-400"
      />
    </label>
  );
}
