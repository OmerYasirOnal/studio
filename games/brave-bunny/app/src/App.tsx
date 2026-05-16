import { useEffect } from 'react';
import Game from './render/Game';
import Boot from './ui/Boot';
import Lobby from './ui/Lobby';
import HUD from './ui/HUD';
import DraftModal from './ui/DraftModal';
import EndRunSummary from './ui/EndRunSummary';
import Joystick from './ui/Joystick';
import { useRunStore } from './state/runStore';
import { useMetaStore } from './state/metaStore';
import { audio } from './audio/AudioBus';
import './ui/styles.css';

export default function App() {
  const phase = useRunStore((s) => s.phase);
  const load = useMetaStore((s) => s.load);

  useEffect(() => {
    audio.init();
    load();
  }, [load]);

  return (
    <>
      <Game />
      {phase === 'run' && <HUD />}
      {phase === 'run' && <Joystick />}
      {phase === 'boot' && <Boot />}
      {phase === 'lobby' && <Lobby />}
      {phase === 'draft' && <DraftModal />}
      {phase === 'endrun' && <EndRunSummary />}
    </>
  );
}
