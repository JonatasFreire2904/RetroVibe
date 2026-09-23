import { useState } from "react";

interface ParticipantsLineChartProps {
  series: { sessionLabel: string; participants: number }[];
}

const WIDTH = 760;
const HEIGHT = 180;
const PADDING = 24;

export function ParticipantsLineChart({ series }: ParticipantsLineChartProps) {
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);
  if (series.length === 0) {
    return <p className="py-10 text-center text-sm text-slate-400">Sem dados suficientes ainda.</p>;
  }

  const maxValue = Math.max(...series.map((s) => s.participants), 1);
  const stepX = series.length > 1 ? (WIDTH - PADDING * 2) / (series.length - 1) : 0;

  const points = series.map((point, index) => {
    const x = PADDING + index * stepX;
    const y = HEIGHT - PADDING - (point.participants / maxValue) * (HEIGHT - PADDING * 2);
    return { x, y, ...point };
  });

  const linePath = points.map((p, i) => `${i === 0 ? "M" : "L"}${p.x},${p.y}`).join(" ");

  return (
    <svg viewBox={`0 0 ${WIDTH} ${HEIGHT}`} className="w-full" role="img" aria-label="Participantes por sessão">
      <line x1={PADDING} y1={HEIGHT - PADDING} x2={WIDTH - PADDING} y2={HEIGHT - PADDING} stroke="#E2E8F0" strokeWidth={1} />
      <path d={linePath} fill="none" stroke="#22D3EE" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round" />
      {points.map((p, index) => (
        <g key={`${p.sessionLabel}-${index}`} onMouseEnter={() => setHoveredIndex(index)} onMouseLeave={() => setHoveredIndex(null)}
          onFocus={() => setHoveredIndex(index)} onBlur={() => setHoveredIndex(null)} tabIndex={0}
          aria-label={`${p.sessionLabel}: ${p.participants} participantes`}>
          <circle cx={p.x} cy={p.y} r={12} fill="transparent" />
          <circle cx={p.x} cy={p.y} r={hoveredIndex === index ? 6 : 4} fill="#06B6D4" />
          {hoveredIndex === index && <g>
            <rect x={Math.max(2, Math.min(WIDTH - 108, p.x - 54))} y={Math.max(2, p.y - 32)} width={108} height={24} rx={6} fill="#1e293b" />
            <text x={Math.max(56, Math.min(WIDTH - 54, p.x))} y={Math.max(18, p.y - 16)} textAnchor="middle" fontSize={11} fill="white">
              {p.participants} participantes
            </text>
          </g>}
        </g>
      ))}
      {points.map((p) => (
        <text key={`${p.sessionLabel}-label`} x={p.x} y={HEIGHT - 4} fontSize={10} textAnchor="middle" fill="#94A3B8" fontFamily="monospace">
          {p.sessionLabel}
        </text>
      ))}
    </svg>
  );
}
