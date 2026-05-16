interface Props {
  value: number;
  max: number;
  variant?: 'hp' | 'xp';
}

export default function Bar({ value, max, variant = 'hp' }: Props) {
  const pct = Math.max(0, Math.min(1, value / max));
  const lowHp = variant === 'hp' && pct < 0.3;
  return (
    <div className={`bar ${variant === 'xp' ? 'bar--xp' : 'bar--hp'}`}>
      <div
        className={`bar__fill ${lowHp ? 'bar--low' : ''}`}
        style={{ transform: `scaleX(${pct})` }}
      />
    </div>
  );
}
