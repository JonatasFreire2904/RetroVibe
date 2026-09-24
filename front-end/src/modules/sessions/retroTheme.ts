import type { SessionBoard, SessionColumn } from "@/shared/types";

export interface RetroPalette {
  background: string;
  border: string;
  accent: string;
  card: string;
}

export interface RetroColumnAppearance extends RetroPalette {
  label: string;
  icon: string;
  prompt: string;
}

interface ColumnCopy {
  label: string;
  icon: string;
  prompt?: string;
}

export interface RetroTheme {
  key: string;
  icon: string;
  shortLabel?: string;
  decor: "flags" | "lights" | "eggs" | "hearts" | "sparkles";
  addLabel: string;
  cardIcon: string;
  palettes: RetroPalette[];
  overrides: Record<string, Record<string, ColumnCopy>>;
}

const basePrompts: Record<string, string> = {
  start: "O que devemos começar?",
  stop: "O que devemos parar?",
  continue: "O que devemos manter?",
  keep: "O que está funcionando bem?",
  more: "O que devemos ampliar?",
  less: "O que devemos reduzir?",
  liked: "O que curtimos essa sprint?",
  learned: "O que aprendemos?",
  lacked: "O que sentimos falta?",
  "longed-for": "O que desejamos para o futuro?",
  notas: "O que você gostaria de compartilhar?",
};

const themes: Record<string, RetroTheme> = {
  "festa-junina": {
    key: "festa-junina", icon: "🎪", decor: "flags", addLabel: "Adicionar ao paiol", cardIcon: "🌽",
    palettes: [
      { background: "#fff3e2", border: "#f76b31", accent: "#f15c1c", card: "#fff8ec" },
      { background: "#f1faff", border: "#bfdff0", accent: "#4296cd", card: "#f6fcff" },
      { background: "#f6fbef", border: "#d1e9cc", accent: "#65a878", card: "#fafff5" },
      { background: "#fff2f5", border: "#f4c9d6", accent: "#da7392", card: "#fff9fb" },
      { background: "#f6f0ff", border: "#d9cafa", accent: "#9065cb", card: "#fbf8ff" },
    ],
    overrides: {
      "start-stop-continue": {
        start: { label: "Acender a Fogueira!", icon: "🔥" },
        stop: { label: "Apagar o Fogo", icon: "💧" },
        continue: { label: "Continuar no Baile!", icon: "🎶" },
      },
      starfish: {
        keep: { label: "Manter o Arraiá!", icon: "🎉" },
        more: { label: "Mais Arrasta-pé!", icon: "🎶" },
        less: { label: "Menos Perrengue", icon: "🌽" },
        stop: { label: "Apagar o Fogo", icon: "💧" },
        start: { label: "Acender a Fogueira!", icon: "🔥" },
      },
      "4ls": {
        liked: { label: "Melhor do Arraiá", icon: "🎉" },
        learned: { label: "Aprendi na Quadrilha", icon: "🎶" },
        lacked: { label: "Faltou na Festa", icon: "🌽" },
        "longed-for": { label: "Desejo para o Próximo Arraiá", icon: "✨" },
      },
    },
  },
  natal: {
    key: "natal", icon: "🎄", decor: "lights", addLabel: "Escrever carta ao Noel", cardIcon: "🎁",
    palettes: [
      { background: "#e7f8eb", border: "#2d823e", accent: "#286c37", card: "#f7fff8" },
      { background: "#dff2ff", border: "#a6d1ef", accent: "#2564a8", card: "#f4fbff" },
      { background: "#e3f4e5", border: "#bbdabc", accent: "#99733b", card: "#f7fff8" },
      { background: "#e3f6e6", border: "#b5dabd", accent: "#b69c43", card: "#f8fff9" },
      { background: "#fff1e8", border: "#f1cfb7", accent: "#bf693e", card: "#fffaf4" },
    ],
    overrides: {
      "start-stop-continue": {
        start: { label: "Acender as Luzes", icon: "✨" },
        stop: { label: "Deixar no Passado", icon: "🎁" },
        continue: { label: "Manter a Magia", icon: "🎄" },
      },
      starfish: {
        keep: { label: "Manter a Magia", icon: "🎄" },
        more: { label: "Mais Espírito Natalino", icon: "🎁" },
        less: { label: "Menos Correria", icon: "❄️" },
        stop: { label: "Deixar no Passado", icon: "🛷" },
        start: { label: "Acender as Luzes", icon: "✨" },
      },
      "4ls": {
        liked: { label: "Hora da Ceia!", icon: "🎄" },
        learned: { label: "Ensaio da Cantiga", icon: "🎶" },
        lacked: { label: "Esqueceu o Presente", icon: "🎁" },
        "longed-for": { label: "Lista do Amigo Oculto", icon: "📝" },
      },
    },
  },
  pascoa: {
    key: "pascoa", icon: "🐰", decor: "eggs", addLabel: "Esconder um ovo", cardIcon: "🐰",
    palettes: [
      { background: "#e9f9ee", border: "#4a9b4d", accent: "#31813c", card: "#f8fff9" },
      { background: "#fcf0fd", border: "#ead6f0", accent: "#a975b8", card: "#fff8ff" },
      { background: "#fff0f8", border: "#f5d2e8", accent: "#df7ba8", card: "#fff9fc" },
      { background: "#fff1f6", border: "#f1d1db", accent: "#db7e96", card: "#fff9fb" },
      { background: "#fff6ee", border: "#f4dfcc", accent: "#e79770", card: "#fffbf6" },
    ],
    overrides: {
      "start-stop-continue": {
        start: { label: "Começar a Caça", icon: "🥚" },
        stop: { label: "Sair da Toca!", icon: "🐇" },
        continue: { label: "Manter a Alegria", icon: "🐰" },
      },
      starfish: {
        keep: { label: "Cesta Cheia", icon: "🧺" },
        more: { label: "Mais Chocolate!", icon: "🍫" },
        less: { label: "Sem Tristeza!", icon: "🌈" },
        stop: { label: "Sair da Toca!", icon: "🐇", prompt: "O que devemos eliminar?" },
        start: { label: "Caça aos Ovos", icon: "🥚", prompt: "O que deveríamos iniciar?" },
      },
      "4ls": {
        liked: { label: "Ovo Favorito", icon: "🥚" },
        learned: { label: "Surpresa na Cesta", icon: "🧺" },
        lacked: { label: "Faltou Chocolate", icon: "🍫" },
        "longed-for": { label: "Desejo de Páscoa", icon: "🐰" },
      },
    },
  },
  "dia-dos-namorados": {
    key: "dia-dos-namorados", icon: "💕", shortLabel: "Namorados", decor: "hearts", addLabel: "Declarar amor", cardIcon: "💕",
    palettes: [
      { background: "#fff0f4", border: "#e06a9a", accent: "#d74782", card: "#fff9fb" },
      { background: "#fff4f1", border: "#f2c4b7", accent: "#e67767", card: "#fffbf9" },
      { background: "#f8efff", border: "#dfcbf4", accent: "#a56bce", card: "#fdf9ff" },
      { background: "#fff1f7", border: "#f3d0e1", accent: "#cf6596", card: "#fffafd" },
      { background: "#fff8ef", border: "#f4ddc2", accent: "#d9955d", card: "#fffdf8" },
    ],
    overrides: {
      "start-stop-continue": {
        start: { label: "Declare seu Amor!", icon: "💌", prompt: "O que devemos começar a fazer?" },
        stop: { label: "Cortar o Ciúme", icon: "💔", prompt: "O que devemos parar de fazer?" },
        continue: { label: "Continuar Apaixonado!", icon: "🌹", prompt: "O que devemos continuar fazendo?" },
      },
      starfish: {
        keep: { label: "Continuar Apaixonado!", icon: "🌹" },
        more: { label: "Mais Carinho!", icon: "💖" },
        less: { label: "Menos Ciúme", icon: "💞" },
        stop: { label: "Cortar o Ciúme", icon: "💔" },
        start: { label: "Declare seu Amor!", icon: "💌" },
      },
      "4ls": {
        liked: { label: "Amei", icon: "💕" },
        learned: { label: "Aprendi a Amar", icon: "💡" },
        lacked: { label: "Senti Falta", icon: "💔" },
        "longed-for": { label: "Sonho a Dois", icon: "🌹" },
      },
    },
  },
  "sem-tema": {
    key: "sem-tema", icon: "✨", decor: "sparkles", addLabel: "Adicionar card", cardIcon: "✦",
    palettes: [
      { background: "#eff7ff", border: "#bcd8f8", accent: "#3b82f6", card: "#f8fbff" },
      { background: "#fff0f5", border: "#f3cada", accent: "#ec4899", card: "#fff9fc" },
      { background: "#f2f0ff", border: "#d9d0f6", accent: "#8b5cf6", card: "#faf9ff" },
      { background: "#edfaf9", border: "#c3e7e1", accent: "#14b8a6", card: "#f8fffd" },
      { background: "#fff6e9", border: "#f4e0bd", accent: "#f59e0b", card: "#fffcf7" },
    ],
    overrides: {
      "start-stop-continue": {
        start: { label: "Começar", icon: "🚀" },
        stop: { label: "Parar", icon: "🛑" },
        continue: { label: "Continuar", icon: "💪" },
      },
      "4ls": {
        liked: { label: "Gostamos", icon: "⭐" },
        learned: { label: "Aprendemos", icon: "🧠" },
        lacked: { label: "Faltou", icon: "🔍" },
        "longed-for": { label: "Desejamos", icon: "🌟" },
      },
    },
  },
};

export function getRetroTheme(key: string): RetroTheme {
  return themes[key] ?? themes["sem-tema"];
}

export function getRetroColumnAppearance(board: SessionBoard, column: SessionColumn, index: number): RetroColumnAppearance {
  const theme = getRetroTheme(board.theme.key);
  const palette = theme.palettes[index % theme.palettes.length];
  const override = theme.overrides[board.template.key]?.[column.key];
  return {
    ...palette,
    label: override?.label ?? column.label,
    icon: override?.icon ?? column.icon,
    prompt: override?.prompt ?? basePrompts[column.key] ?? "O que você gostaria de compartilhar?",
  };
}
