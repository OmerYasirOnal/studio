import { useEffect } from 'react';
import { useRunStore } from '@/state/runStore';

export default function Boot() {
  const setPhase = useRunStore((s) => s.setPhase);
  useEffect(() => {
    const t = setTimeout(() => setPhase('lobby'), 1500);
    return () => clearTimeout(t);
  }, [setPhase]);

  return (
    <div className="overlay overlay--blocking">
      <div className="card card--lobby">
        <div style={{ fontSize: 64, marginBottom: 16 }}>🐰</div>
        <h1 className="title lobby-title">Brave Bunny</h1>
        <p style={{ color: 'var(--text-mute)', margin: 0 }}>Loading…</p>
      </div>
    </div>
  );
}
