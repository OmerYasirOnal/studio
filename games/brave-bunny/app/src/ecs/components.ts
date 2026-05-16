import type { Object3D } from 'three';

export type Vec3 = { x: number; y: number; z: number };

export type Archetype = 'hero' | 'slime' | 'wolf' | 'mushroom' | 'projectile' | 'pickup' | 'vfx';

export type WeaponKind = 'spear' | 'sling';

export interface WeaponInstance {
  kind: WeaponKind;
  damage: number;
  tickInterval: number;
  cooldown: number;
  level: number;
}

export interface Entity {
  position?: Vec3;
  velocity?: Vec3;
  rotationY?: number;
  meshRef?: Object3D | null;
  archetype?: Archetype;
  modelKey?: string;
  hp?: number;
  maxHp?: number;
  damage?: number;
  team?: 'hero' | 'enemy';
  movement?: 'seek-hero' | 'projectile' | 'pickup-magnet' | 'none';
  speed?: number;
  ttl?: number;
  weapons?: WeaponInstance[];
  xpValue?: number;
  hitFlashTime?: number;
}
