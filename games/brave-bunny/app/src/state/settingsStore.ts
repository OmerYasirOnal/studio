import { create } from 'zustand';

interface SettingsState {
  bgmVolume: number;
  sfxVolume: number;
  setBgm: (v: number) => void;
  setSfx: (v: number) => void;
}

export const useSettingsStore = create<SettingsState>((set) => ({
  bgmVolume: 0.4,
  sfxVolume: 0.7,
  setBgm: (v) => set({ bgmVolume: v }),
  setSfx: (v) => set({ sfxVolume: v }),
}));
