import wavesData from '@/data/waves.json';
import { world } from '@/ecs/world';
import { heroQuery, enemyQuery } from '@/ecs/queries';
import { useRunStore } from '@/state/runStore';
import type { Archetype } from '@/ecs/components';

let spawnTimer = 0;
const MAX_ENEMIES = 60;

function currentWave(time: number) {
  for (const w of wavesData.waves) {
    if (time >= w.timeStart && time < w.timeEnd) return w;
  }
  return wavesData.waves[wavesData.waves.length - 1];
}

function pickArchetype(weights: Record<string, number>): Archetype {
  const total = Object.values(weights).reduce((a, b) => a + b, 0);
  let r = Math.random() * total;
  for (const [arch, w] of Object.entries(weights)) {
    r -= w;
    if (r <= 0) return arch as Archetype;
  }
  return 'slime';
}

function spawnAt(angleRad: number, dist: number): void {
  const hero = heroQuery.first;
  if (!hero?.position) return;
  const time = useRunStore.getState().time;
  const wave = currentWave(time);
  const archetype = pickArchetype(wave.archetypeWeights);
  const stats = (
    wavesData.archetypes as Record<
      string,
      { hp: number; speed: number; damage: number; xpValue: number }
    >
  )[archetype];

  world.add({
    archetype,
    team: 'enemy',
    position: {
      x: hero.position.x + Math.cos(angleRad) * dist,
      y: 0,
      z: hero.position.z + Math.sin(angleRad) * dist,
    },
    velocity: { x: 0, y: 0, z: 0 },
    rotationY: 0,
    hp: stats.hp,
    maxHp: stats.hp,
    speed: stats.speed,
    damage: stats.damage,
    xpValue: stats.xpValue,
    movement: 'seek-hero',
  });
}

export function tickSpawn(delta: number): void {
  const time = useRunStore.getState().time;
  const wave = currentWave(time);

  spawnTimer -= delta;
  if (spawnTimer > 0) return;
  spawnTimer = wave.spawnInterval;

  // Cap active enemies
  if (enemyQuery.size >= MAX_ENEMIES) return;

  const angle = Math.random() * Math.PI * 2;
  const dist = 12 + Math.random() * 4;
  spawnAt(angle, dist);
}

export function resetSpawn(): void {
  spawnTimer = 0;
}
