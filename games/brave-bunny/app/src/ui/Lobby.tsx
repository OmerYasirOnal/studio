import { useState } from 'react';
import { useRunStore } from '@/state/runStore';
import { audio } from '@/audio/AudioBus';
import Profile from './Profile';
import Settings from './Settings';

export default function Lobby() {
  const setPhase = useRunStore((s) => s.setPhase);
  const reset = useRunStore((s) => s.reset);
  const [modal, setModal] = useState<'profile' | 'settings' | null>(null);

  const startRun = () => {
    audio.play('click');
    reset();
    setPhase('run');
    audio.startBgm();
  };

  return (
    <div className="overlay overlay--blocking">
      <div className="card card--lobby">
        <h1 className="title">Brave Bunny</h1>
        <button className="btn btn--cta" onClick={startRun}>
          ▶ PLAY
        </button>
        <div style={{ height: 16 }} />
        <div className="icon-row">
          <button
            className="icon-btn"
            onClick={() => {
              audio.play('click');
              setModal('profile');
            }}
            aria-label="Profile"
          >
            👤
          </button>
          <button
            className="icon-btn"
            onClick={() => {
              audio.play('click');
              setModal('settings');
            }}
            aria-label="Settings"
          >
            ⚙
          </button>
        </div>
      </div>
      {modal === 'profile' && <Profile onClose={() => setModal(null)} />}
      {modal === 'settings' && <Settings onClose={() => setModal(null)} />}
    </div>
  );
}
