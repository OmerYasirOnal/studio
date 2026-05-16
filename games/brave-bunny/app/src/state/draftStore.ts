import { create } from 'zustand';

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
  },
  reset: () => set({ offers: [], taken: { ...initialTaken } }),
}));
