// Stub for Stream S5. Real implementation lands in S9.
import { create } from 'zustand';

interface MetaStubState {
  totalRuns: number;
  bestKills: number;
  longestRun: number;
  totalGold: number;
  totalXpEarned: number;
  loaded: boolean;
  load: () => Promise<void>;
  save: () => Promise<void>;
  bankRun: (r: { kills: number; time: number; xpEarned: number; gold: number }) => void;
  resetSave: () => Promise<void>;
}

export const useMetaStore = create<MetaStubState>((set) => ({
  totalRuns: 0,
  bestKills: 0,
  longestRun: 0,
  totalGold: 0,
  totalXpEarned: 0,
  loaded: true,
  load: async () => {},
  save: async () => {},
  bankRun: () => {},
  resetSave: async () => {
    set({ totalRuns: 0, bestKills: 0, longestRun: 0, totalGold: 0, totalXpEarned: 0 });
  },
}));
