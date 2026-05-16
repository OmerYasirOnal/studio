import { useFrame } from '@react-three/fiber';
import { useRunStore } from '@/state/runStore';
import { tickWeapons } from './weapons';
import { tickProjectiles } from './projectiles';
import { tickEnemyAI } from './enemyAI'; // from S7 — will exist when wired
import { tickSpawn } from './spawn'; // from S7
import { tickLifecycle } from './lifecycle'; // from S8

export function useRunLoop(): null {
  const phase = useRunStore((s) => s.phase);

  useFrame((_, delta) => {
    if (phase !== 'run') return;
    const dt = Math.min(delta, 0.05); // clamp big spikes

    tickSpawn(dt);
    tickEnemyAI(dt);
    tickWeapons(dt);
    tickProjectiles(dt);
    tickLifecycle(dt);

    useRunStore.setState((s) => ({ time: s.time + dt }));
  });
  return null;
}

export function RunLoop() {
  useRunLoop();
  return null;
}
