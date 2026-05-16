import { world } from '@/ecs/world';
import { enemyQuery, heroQuery, pickupQuery } from '@/ecs/queries';
import { useRunStore } from '@/state/runStore';
import { audio } from '@/audio/AudioBus';

const MAGNET_RADIUS_SQ = 9; // default 3u magnet
const PICKUP_HIT_RADIUS_SQ = 1.0; // 1.0u
let magnetRadiusSq = MAGNET_RADIUS_SQ;

export function setMagnetRadius(r: number): void {
  magnetRadiusSq = r * r;
}

const ENEMY_DEATH_ANIM_DURATION = 0.6;

export function tickLifecycle(delta: number): void {
  // Enemy death → mark as dying, spawn pickup, increment kills
  // (actual world.remove deferred until deathTimer expires so death anim plays)
  for (const e of enemyQuery) {
    if (e.hp != null && e.hp <= 0 && !e.dying) {
      e.dying = true;
      e.deathTimer = ENEMY_DEATH_ANIM_DURATION;
      if (e.position && e.xpValue != null) {
        world.add({
          archetype: 'pickup',
          team: undefined,
          position: { ...e.position },
          velocity: { x: 0, y: 0, z: 0 },
          xpValue: e.xpValue,
          movement: 'pickup-magnet',
        });
      }
      useRunStore.getState().incKills();
    }
  }

  // Tick dying timers + remove when done
  for (const e of enemyQuery) {
    if (e.dying && e.deathTimer != null) {
      e.deathTimer -= delta;
      if (e.deathTimer <= 0) {
        world.remove(e);
      }
    }
  }

  // Pickup magnet + collect
  const hero = heroQuery.first;
  if (!hero?.position) return;

  for (const p of pickupQuery) {
    if (!p.position || !p.velocity) continue;
    const dx = hero.position.x - p.position.x;
    const dz = hero.position.z - p.position.z;
    const distSq = dx * dx + dz * dz;

    if (distSq < magnetRadiusSq) {
      const dist = Math.sqrt(distSq);
      const speed = 10;
      if (dist > 0.05) {
        p.position.x += (dx / dist) * speed * delta;
        p.position.z += (dz / dist) * speed * delta;
      }
    }

    if (distSq < PICKUP_HIT_RADIUS_SQ) {
      if (p.xpValue != null) {
        const prevLevel = useRunStore.getState().level;
        useRunStore.getState().addXp(p.xpValue);
        const newLevel = useRunStore.getState().level;
        if (newLevel > prevLevel) {
          audio.play('levelup');
          useRunStore.getState().setPhase('draft');
        } else {
          audio.play('gem');
        }
      }
      world.remove(p);
    }
  }

  // Hero death
  if (hero.hp != null && hero.hp <= 0) {
    audio.play('death');
    useRunStore.getState().setPhase('endrun');
  }

  // 5-min timeout → endrun
  const time = useRunStore.getState().time;
  if (time >= 300 && useRunStore.getState().phase === 'run') {
    useRunStore.getState().setPhase('endrun');
  }
}
