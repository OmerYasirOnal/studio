import { create } from 'zustand';

interface DamageEffect {
  id: number;
  x: number;
  y: number;
  z: number;
  amount: number;
  age: number; // seconds since spawn
}

interface SparkEffect {
  id: number;
  x: number;
  y: number;
  z: number;
  age: number;
}

interface BurstEffect {
  id: number;
  x: number;
  y: number;
  z: number;
  age: number;
}

interface PoofEffect {
  id: number;
  x: number;
  y: number;
  z: number;
  age: number;
}

interface VfxState {
  damages: DamageEffect[];
  sparks: SparkEffect[];
  bursts: BurstEffect[];
  poofs: PoofEffect[];
  emitDamage: (x: number, y: number, z: number, amount: number) => void;
  emitSpark: (x: number, y: number, z: number) => void;
  emitBurst: (x: number, y: number, z: number) => void;
  emitPoof: (x: number, y: number, z: number) => void;
  tick: (delta: number) => void;
}

let nextId = 1;
const DAMAGE_TTL = 0.8;
const SPARK_TTL = 0.3;
const BURST_TTL = 0.6;
const POOF_TTL = 0.4;
const MAX_PER_KIND = 30;

export const useVfxStore = create<VfxState>((set) => ({
  damages: [],
  sparks: [],
  bursts: [],
  poofs: [],
  emitDamage: (x, y, z, amount) =>
    set((s) => ({
      damages: [...s.damages, { id: nextId++, x, y, z, amount, age: 0 }].slice(-MAX_PER_KIND),
    })),
  emitSpark: (x, y, z) =>
    set((s) => ({
      sparks: [...s.sparks, { id: nextId++, x, y, z, age: 0 }].slice(-MAX_PER_KIND),
    })),
  emitBurst: (x, y, z) =>
    set((s) => ({
      bursts: [...s.bursts, { id: nextId++, x, y, z, age: 0 }].slice(-MAX_PER_KIND),
    })),
  emitPoof: (x, y, z) =>
    set((s) => ({
      poofs: [...s.poofs, { id: nextId++, x, y, z, age: 0 }].slice(-MAX_PER_KIND),
    })),
  tick: (delta) =>
    set((s) => ({
      damages: s.damages
        .map((d) => ({ ...d, age: d.age + delta }))
        .filter((d) => d.age < DAMAGE_TTL),
      sparks: s.sparks.map((d) => ({ ...d, age: d.age + delta })).filter((d) => d.age < SPARK_TTL),
      bursts: s.bursts.map((d) => ({ ...d, age: d.age + delta })).filter((d) => d.age < BURST_TTL),
      poofs: s.poofs.map((d) => ({ ...d, age: d.age + delta })).filter((d) => d.age < POOF_TTL),
    })),
}));
