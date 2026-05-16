import Game from './render/Game';
import Joystick from './ui/Joystick';
import { useRunStore } from './state/runStore';
import { useEffect } from 'react';

export default function App() {
  const phase = useRunStore((s) => s.phase);
  const setPhase = useRunStore((s) => s.setPhase);

  // Skip boot for now (S5 will add proper screens)
  useEffect(() => {
    if (phase === 'boot') setPhase('run');
  }, [phase, setPhase]);

  return (
    <>
      <Game />
      {phase === 'run' && <Joystick />}
    </>
  );
}
