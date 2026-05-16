import { world } from './world';

export const heroQuery = world.with('archetype', 'position', 'hp').where(e => e.archetype === 'hero');
export const enemyQuery = world.with('archetype', 'position', 'hp', 'team').where(e => e.team === 'enemy');
export const projectileQuery = world.with('archetype', 'position', 'velocity').where(e => e.archetype === 'projectile');
export const pickupQuery = world.with('archetype', 'position').where(e => e.archetype === 'pickup');
export const ttlQuery = world.with('ttl');
