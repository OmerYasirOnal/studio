import { world } from '@/ecs/world';
import { projectileQuery, enemyQuery } from '@/ecs/queries';
import { audio } from '@/audio/AudioBus';
import { useVfxStore } from '@/state/vfxStore';

export function tickProjectiles(delta: number): void {
  for (const p of projectileQuery) {
    if (!p.position || !p.velocity || p.ttl == null) continue;
    p.position.x += p.velocity.x * delta;
    p.position.z += p.velocity.z * delta;
    p.ttl -= delta;

    if (p.ttl <= 0) {
      world.remove(p);
      continue;
    }

    // Collision with enemies (AABB-ish: 0.3u projectile, 0.5u enemy)
    for (const e of enemyQuery) {
      if (!e.position || e.hp == null || e.dying) continue;
      const dx = e.position.x - p.position.x;
      const dz = e.position.z - p.position.z;
      if (dx * dx + dz * dz < 0.64) {
        e.hp -= p.damage ?? 0;
        e.hitFlashTime = 0.15;
        audio.play('hit');
        if (e.position) {
          useVfxStore
            .getState()
            .emitDamage(e.position.x, e.position.y, e.position.z, p.damage ?? 0);
          useVfxStore.getState().emitSpark(e.position.x, e.position.y, e.position.z);
        }
        world.remove(p);
        break;
      }
    }
  }
}
