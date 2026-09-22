import type { RetroTheme } from "../retroTheme";

export function ThemeTrim({ decor, small = false }: { decor: RetroTheme["decor"]; small?: boolean }) {
  return (
    <div className={`retro-trim retro-trim--${decor}${small ? " retro-trim--small" : ""}`} aria-hidden="true">
      {Array.from({ length: small ? 17 : decor === "hearts" ? 24 : 56 }, (_, index) => (
        <span key={index} className={`retro-trim-piece retro-trim-piece--${index % 8}`} />
      ))}
    </div>
  );
}
