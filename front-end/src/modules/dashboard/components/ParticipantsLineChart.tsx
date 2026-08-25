interface ParticipantsLineChartProps {
  series: { sessionLabel: string; participants: number }[];
}

const WIDTH = 760;
const HEIGHT = 180;
const PADDING = 24;

export function ParticipantsLineChart({ series }: ParticipantsLineChartProps) {
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
      {points.map((p) => (
        <circle key={p.sessionLabel} cx={p.x} cy={p.y} r={4} fill="#06B6D4" />
      ))}
      {points.map((p) => (
        <text key={`${p.sessionLabel}-label`} x={p.x} y={HEIGHT - 4} fontSize={10} textAnchor="middle" fill="#94A3B8" fontFamily="monospace">
          {p.sessionLabel}
        </text>
      ))}
    </svg>
  );
}
