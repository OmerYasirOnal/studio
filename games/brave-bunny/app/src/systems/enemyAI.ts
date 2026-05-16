import { enemyQuery, heroQuery } from '@/ecs/queries';
import { world } from '@/ecs/world';
import { audio } from '@/audio/AudioBus';

const HERO_COLLISION_RADIUS = 0.8;
const HERO_DAMAGE_COOLDOWN = 0.5; // each enemy can damage hero at most every 0.5s
const DESPAWN_DIST_SQ = 1600; // 40u

const damageCooldowns = new WeakMap<object, number>();

export function tickEnemyAI(delta: number): void {
  const hero = heroQuery.first;
  if (!hero?.position) return;

  for (const e of enemyQuery) {
    if (!e.position || !e.velocity || e.hp == null) continue;
    if (e.dying) continue; // dying enemies freeze (death anim plays)

    // Despawn if too far
    const dxh = e.position.x - hero.position.x;
    const dzh = e.position.z - hero.position.z;
    const distSqH = dxh * dxh + dzh * dzh;
    if (distSqH > DESPAWN_DIST_SQ) {
      world.remove(e);
      continue;
    }

    // Seek hero
    const dist = Math.sqrt(distSqH);
    if (dist > 0.1) {
      const speed = e.speed ?? 2;
      e.velocity.x = -(dxh / dist) * speed;
      e.velocity.z = -(dzh / dist) * speed;
      e.position.x += e.velocity.x * delta;
      e.position.z += e.velocity.z * delta;
      e.rotationY = Math.atan2(-dxh, -dzh);
    }

    // Hero collision → damage tick
    if (distSqH < HERO_COLLISION_RADIUS * HERO_COLLISION_RADIUS) {
      const cd = damageCooldowns.get(e) ?? 0;
      if (cd <= 0) {
        if (hero.hp != null && e.damage != null) {
          hero.hp -= e.damage;
          audio.play('enemy-hit');
          damageCooldowns.set(e, HERO_DAMAGE_COOLDOWN);
        }
      } else {
        damageCooldowns.set(e, cd - delta);
      }
    }
  }
}
