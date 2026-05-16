import { create } from 'zustand';
import { heroQuery } from '@/ecs/queries';
import { setMagnetRadius } from '@/systems/lifecycle';

export type UpgradeKind = 'spear-dmg' | 'sling-dmg' | 'hp' | 'speed' | 'magnet' | 'attack-rate';

export interface Upgrade {
  kind: UpgradeKind;
  icon: string;
  name: string;
  description: string;
  stacks: number;
  maxStacks: number;
}

const TEMPLATES: Record<UpgradeKind, Omit<Upgrade, 'stacks'>> = {
  'spear-dmg':   { kind: 'spear-dmg',   icon: '🥕', name: 'Carrot Damage',  description: '+5 damage to Carrot Spear', maxStacks: 5 },
  'sling-dmg':   { kind: 'sling-dmg',   icon: '🪨', name: 'Pebble Damage',  description: '+3 damage to Pebble Sling', maxStacks: 5 },
  'hp':          { kind: 'hp',          icon: '❤️', name: 'Health Up',      description: '+20 max HP, fully heal',     maxStacks: 5 },
  'speed':       { kind: 'speed',       icon: '👟', name: 'Speed Up',       description: '+0.5 movement speed',         maxStacks: 3 },
  'magnet':      { kind: 'magnet',      icon: '🧲', name: 'Magnet Up',      description: '+1u magnet radius',           maxStacks: 3 },
  'attack-rate': { kind: 'attack-rate', icon: '⚡', name: 'Attack Speed',   description: '−0.05s tick on both weapons', maxStacks: 4 },
};

interface DraftState {
  offers: Upgrade[];
  taken: Record<UpgradeKind, number>;
  rollOffers: () => void;
  pick: (kind: UpgradeKind) => void;
  reset: () => void;
}

const initialTaken: Record<UpgradeKind, number> = {
  'spear-dmg': 0, 'sling-dmg': 0, 'hp': 0, 'speed': 0, 'magnet': 0, 'attack-rate': 0,
};

export function applyUpgrade(kind: UpgradeKind): void {
  const hero = heroQuery.first;
  if (!hero) return;

  switch (kind) {
    case 'spear-dmg': {
      const w = hero.weapons?.find((x) => x.kind === 'spear');
      if (w) w.damage += 5;
      break;
    }
    case 'sling-dmg': {
      const w = hero.weapons?.find((x) => x.kind === 'sling');
      if (w) w.damage += 3;
      break;
    }
    case 'hp': {
      if (hero.maxHp != null) hero.maxHp += 20;
      hero.hp = hero.maxHp ?? 100; // full heal
      break;
    }
    case 'speed': {
      if (hero.speed != null) hero.speed += 0.5;
      break;
    }
    case 'magnet': {
      const taken = useDraftStore.getState().taken.magnet + 1;
      setMagnetRadius(2 + taken * 1);
      break;
    }
    case 'attack-rate': {
      if (hero.weapons) {
        for (const w of hero.weapons) {
          w.tickInterval = Math.max(0.1, w.tickInterval - 0.05);
        }
      }
      break;
    }
  }
}

export const useDraftStore = create<DraftState>((set, get) => ({
  offers: [],
  taken: { ...initialTaken },
  rollOffers: () => {
    const taken = get().taken;
    const eligible = Object.values(TEMPLATES).filter(t => taken[t.kind] < t.maxStacks);
    const shuffled = [...eligible].sort(() => Math.random() - 0.5);
    const offers = shuffled.slice(0, 3).map(t => ({ ...t, stacks: taken[t.kind] }));
    set({ offers });
  },
  pick: (kind) => {
    set((s) => ({ taken: { ...s.taken, [kind]: s.taken[kind] + 1 }, offers: [] }));
    applyUpgrade(kind);
  },
  reset: () => set({ offers: [], taken: { ...initialTaken } }),
}));
