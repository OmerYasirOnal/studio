import { useRunStore } from '@/state/runStore';
import { useMetaStore } from '@/state/metaStore';
import { useDraftStore } from '@/state/draftStore';
import { audio } from '@/audio/AudioBus';

export default function EndRunSummary() {
  const { time, kills, level, xp } = useRunStore();
  const setPhase = useRunStore((s) => s.setPhase);
  const reset = useRunStore((s) => s.reset);
  const draftReset = useDraftStore((s) => s.reset);
  const bankRun = useMetaStore((s) => s.bankRun);

  const gold = Math.floor(kills * 1.5 + time / 10);

  // bank once on mount
  if (!(window as unknown as { __runBanked?: boolean }).__runBanked) {
    (window as unknown as { __runBanked?: boolean }).__runBanked = true;
    bankRun({ kills, time, xpEarned: xp, gold });
  }

  const min = Math.floor(time / 60);
  const sec = Math.floor(time % 60)
    .toString()
    .padStart(2, '0');

  const restart = () => {
    (window as unknown as { __runBanked?: boolean }).__runBanked = false;
    audio.play('click');
    reset();
    draftReset();
    setPhase('run');
  };
  const lobby = () => {
    (window as unknown as { __runBanked?: boolean }).__runBanked = false;
    audio.play('click');
    reset();
    draftReset();
    setPhase('lobby');
    audio.stopBgm();
  };

  return (
    <div className="overlay overlay--blocking">
      <div className="card" style={{ minWidth: 320, maxWidth: 420 }}>
        <h2 className="title" style={{ textAlign: 'center' }}>
          Run Complete
        </h2>
        <div className="endrun-stats">
          <div>
            <div className="endrun-stat__label">Kills</div>
            <div className="endrun-stat__value">{kills}</div>
          </div>
          <div>
            <div className="endrun-stat__label">Time</div>
            <div className="endrun-stat__value">
              {min}:{sec}
            </div>
          </div>
          <div>
            <div className="endrun-stat__label">Level</div>
            <div className="endrun-stat__value">{level}</div>
          </div>
          <div>
            <div className="endrun-stat__label">Gold</div>
            <div className="endrun-stat__value">+{gold}</div>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 8, flexDirection: 'column' }}>
          <button className="btn btn--cta" onClick={restart}>
            ▶ RESTART
          </button>
          <button className="btn btn--ghost" onClick={lobby}>
            ⌂ LOBBY
          </button>
        </div>
      </div>
    </div>
  );
}
