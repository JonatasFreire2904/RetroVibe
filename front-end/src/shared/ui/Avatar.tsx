interface AvatarProps {
  name: string;
  color: string;
  size?: "sm" | "md";
}

function initialsOf(name: string): string {
  const parts = name.trim().split(/\s+/);
  const first = parts[0]?.[0] ?? "";
  const second = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? "" : "";
  return (first + second).toUpperCase();
}

export function Avatar({ name, color, size = "md" }: AvatarProps) {
  const dimension = size === "sm" ? "h-6 w-6 text-[10px]" : "h-10 w-10 text-sm";
  return (
    <div
      className={`flex ${dimension} shrink-0 items-center justify-center rounded-full font-semibold text-white`}
      style={{ backgroundColor: color }}
      title={name}
    >
      {initialsOf(name)}
    </div>
  );
}
