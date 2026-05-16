import { create } from 'zustand';

export type Phase = 'boot' | 'lobby' | 'run' | 'draft' | 'endrun';

interface Input {
  x: number; // -1..1 joystick x
  y: number; // -1..1 joystick y
  active: boolean;
}

interface RunState {
  phase: Phase;
  time: number; // seconds in current run
  kills: number;
  level: number;
  xp: number;
  xpForNext: number;
  input: Input;
  setPhase: (p: Phase) => void;
  setInput: (i: Partial<Input>) => void;
  incKills: () => void;
  addXp: (n: number) => void;
  reset: () => void;
}

const INITIAL: Omit<RunState, 'setPhase' | 'setInput' | 'incKills' | 'addXp' | 'reset'> = {
  phase: 'boot',
  time: 0,
  kills: 0,
  level: 1,
  xp: 0,
  xpForNext: 23,
  input: { x: 0, y: 0, active: false },
};

export const useRunStore = create<RunState>((set) => ({
  ...INITIAL,
  setPhase: (p) => set({ phase: p }),
  setInput: (i) => set((s) => ({ input: { ...s.input, ...i } })),
  incKills: () => set((s) => ({ kills: s.kills + 1 })),
  addXp: (n) =>
    set((s) => {
      let xp = s.xp + n;
      let level = s.level;
      let xpForNext = s.xpForNext;
      while (xp >= xpForNext) {
        xp -= xpForNext;
        level += 1;
        xpForNext = 15 + level * 8;
      }
      return { xp, level, xpForNext };
    }),
  reset: () => set({ ...INITIAL, phase: 'lobby' }),
}));
