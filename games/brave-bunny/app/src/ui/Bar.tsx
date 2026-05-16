interface Props {
  value: number;
  max: number;
  variant?: 'hp' | 'xp';
}

export default function Bar({ value, max, variant = 'hp' }: Props) {
  const pct = Math.max(0, Math.min(1, value / max));
  return (
    <div className={`bar ${variant === 'xp' ? 'bar--xp' : ''}`}>
      <div className="bar__fill" style={{ transform: `scaleX(${pct})` }} />
    </div>
  );
}
