import { world } from '@/ecs/world';
import { enemyQuery, heroQuery } from '@/ecs/queries';
import { audio } from '@/audio/AudioBus';
import { useVfxStore } from '@/state/vfxStore';
import type { Entity, WeaponKind } from '@/ecs/components';

const WEAPON_DEFAULTS: Record<WeaponKind, { damage: number; tickInterval: number }> = {
  spear: { damage: 15, tickInterval: 0.6 },
  sling: { damage: 10, tickInterval: 0.4 },
};

export function isInCone(
  fromX: number,
  fromZ: number,
  forwardX: number,
  forwardZ: number,
  targetX: number,
  targetZ: number,
  range: number,
  coneCosThreshold: number,
): boolean {
  const dx = targetX - fromX;
  const dz = targetZ - fromZ;
  const d = Math.hypot(dx, dz);
  if (d > range) return false;
  const dot = (dx * forwardX + dz * forwardZ) / Math.max(d, 0.001);
  return dot >= coneCosThreshold;
}

function distSq(a: { x: number; z: number }, b: { x: number; z: number }): number {
  const dx = a.x - b.x;
  const dz = a.z - b.z;
  return dx * dx + dz * dz;
}

function spawnProjectile(
  from: { x: number; y: number; z: number },
  target: { x: number; y: number; z: number },
  damage: number,
): void {
  const dx = target.x - from.x;
  const dz = target.z - from.z;
  const dist = Math.hypot(dx, dz);
  const speed = 12;
  world.add({
    archetype: 'projectile',
    position: { x: from.x, y: from.y + 0.3, z: from.z },
    velocity: {
      x: dist === 0 ? 0 : (dx / dist) * speed,
      y: 0,
      z: dist === 0 ? 0 : (dz / dist) * speed,
    },
    damage,
    team: 'hero',
    ttl: 2,
  });
}

export function tickWeapons(delta: number): void {
  const hero = heroQuery.first;
  if (!hero?.position) return;

  // Auto-attack with default weapons if none equipped
  if (!hero.weapons || hero.weapons.length === 0) {
    hero.weapons = [
      { kind: 'spear', ...WEAPON_DEFAULTS.spear, cooldown: 0, level: 1 },
      { kind: 'sling', ...WEAPON_DEFAULTS.sling, cooldown: 0, level: 1 },
    ];
  }

  for (const weapon of hero.weapons) {
    weapon.cooldown -= delta;
    if (weapon.cooldown > 0) continue;
    weapon.cooldown = weapon.tickInterval;

    if (weapon.kind === 'spear') {
      // Cone front of hero
      const heroRotY = hero.rotationY ?? 0;
      const forwardX = Math.sin(heroRotY);
      const forwardZ = Math.cos(heroRotY);
      const range = 2.5;
      const coneCosThreshold = Math.cos(Math.PI / 6); // 30° half-angle = 60° cone

      for (const e of enemyQuery) {
        if (!e.position || e.hp == null || e.dying) continue;
        if (
          !isInCone(
            hero.position.x,
            hero.position.z,
            forwardX,
            forwardZ,
            e.position.x,
            e.position.z,
            range,
            coneCosThreshold,
          )
        )
          continue;
        e.hp -= weapon.damage;
        e.hitFlashTime = 0.15;
        audio.play('hit');
        if (e.position) {
          useVfxStore
            .getState()
            .emitDamage(e.position.x, e.position.y, e.position.z, weapon.damage);
          useVfxStore.getState().emitSpark(e.position.x, e.position.y, e.position.z);
        }
      }
    } else if (weapon.kind === 'sling') {
      // Find nearest enemy, spawn projectile
      let nearest: Entity | null = null;
      let nearestDistSq = 64; // 8u range squared
      for (const e of enemyQuery) {
        if (!e.position || e.hp == null || e.dying) continue;
        const d = distSq(e.position, hero.position);
        if (d < nearestDistSq) {
          nearest = e;
          nearestDistSq = d;
        }
      }
      if (nearest?.position) {
        spawnProjectile(hero.position, nearest.position, weapon.damage);
      }
    }
  }
}
