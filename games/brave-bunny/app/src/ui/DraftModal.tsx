import { useEffect } from 'react';
import { useRunStore } from '@/state/runStore';
import { useDraftStore } from '@/state/draftStore';
import { audio } from '@/audio/AudioBus';

export default function DraftModal() {
  const offers = useDraftStore((s) => s.offers);
  const rollOffers = useDraftStore((s) => s.rollOffers);
  const pick = useDraftStore((s) => s.pick);
  const setPhase = useRunStore((s) => s.setPhase);

  useEffect(() => {
    if (offers.length === 0) rollOffers();
  }, [offers.length, rollOffers]);

  const onPick = (kind: typeof offers[0]['kind']) => {
    audio.play('draftPick');
    pick(kind);
    setPhase('run');
  };

  return (
    <div className="overlay overlay--blocking">
      <div style={{ maxWidth: 800, width: '100%' }}>
        <h2 className="title" style={{ textAlign: 'center', marginBottom: 16 }}>Level Up!</h2>
        <p style={{ textAlign: 'center', color: 'var(--text-mute)', marginBottom: 24 }}>Pick one upgrade</p>
        <div className="draft-grid">
          {offers.map((up) => (
            <button key={up.kind} className="draft-card" onClick={() => onPick(up.kind)}>
              <div className="draft-card__icon">{up.icon}</div>
              <div className="draft-card__name">{up.name}</div>
              <div className="draft-card__desc">{up.description}</div>
              <div style={{ marginTop: 6, fontSize: 12, color: 'var(--text-mute)' }}>{up.stacks}/{up.maxStacks}</div>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
