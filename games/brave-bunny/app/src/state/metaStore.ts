import { create } from 'zustand';
import { Preferences } from '@capacitor/preferences';

const SAVE_KEY = 'brave-bunny.save.v1';

interface RunResult {
  kills: number;
  time: number;
  xpEarned: number;
  gold: number;
}

interface MetaState {
  loaded: boolean;
  totalRuns: number;
  bestKills: number;
  longestRun: number;
  totalGold: number;
  totalXpEarned: number;
  load: () => Promise<void>;
  save: () => Promise<void>;
  bankRun: (r: RunResult) => void;
  resetSave: () => Promise<void>;
}

const initial = {
  totalRuns: 0,
  bestKills: 0,
  longestRun: 0,
  totalGold: 0,
  totalXpEarned: 0,
};

export const useMetaStore = create<MetaState>((set, get) => ({
  loaded: false,
  ...initial,
  load: async () => {
    try {
      const { value } = await Preferences.get({ key: SAVE_KEY });
      if (value) {
        const parsed = JSON.parse(value);
        set({ ...initial, ...parsed, loaded: true });
      } else {
        set({ loaded: true });
      }
    } catch (e) {
      console.warn('metaStore: load failed', e);
      set({ loaded: true });
    }
  },
  save: async () => {
    const { totalRuns, bestKills, longestRun, totalGold, totalXpEarned } = get();
    try {
      await Preferences.set({
        key: SAVE_KEY,
        value: JSON.stringify({
          version: 1,
          totalRuns,
          bestKills,
          longestRun,
          totalGold,
          totalXpEarned,
        }),
      });
    } catch (e) {
      console.warn('metaStore: save failed', e);
    }
  },
  bankRun: (r) => {
    set((s) => ({
      totalRuns: s.totalRuns + 1,
      bestKills: Math.max(s.bestKills, r.kills),
      longestRun: Math.max(s.longestRun, r.time),
      totalGold: s.totalGold + r.gold,
      totalXpEarned: s.totalXpEarned + r.xpEarned,
    }));
    // Async fire-and-forget
    get().save();
  },
  resetSave: async () => {
    set({ ...initial });
    try {
      await Preferences.remove({ key: SAVE_KEY });
    } catch {
      /* ignore */
    }
  },
}));
