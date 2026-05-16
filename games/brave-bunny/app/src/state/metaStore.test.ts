import { describe, it, expect, beforeEach, vi } from 'vitest';

vi.mock('@capacitor/preferences', () => ({
  Preferences: {
    get: vi.fn(() => Promise.resolve({ value: null })),
    set: vi.fn(() => Promise.resolve()),
    remove: vi.fn(() => Promise.resolve()),
  },
}));

import { useMetaStore } from './metaStore';

describe('metaStore.bankRun', () => {
  beforeEach(async () => {
    await useMetaStore.getState().resetSave();
  });

  it('increments totalRuns', () => {
    useMetaStore.getState().bankRun({ kills: 5, time: 60, xpEarned: 20, gold: 15 });
    expect(useMetaStore.getState().totalRuns).toBe(1);
  });

  it('tracks best kills', () => {
    useMetaStore.getState().bankRun({ kills: 5, time: 60, xpEarned: 20, gold: 15 });
    useMetaStore.getState().bankRun({ kills: 3, time: 30, xpEarned: 8, gold: 5 });
    useMetaStore.getState().bankRun({ kills: 8, time: 90, xpEarned: 30, gold: 22 });
    expect(useMetaStore.getState().bestKills).toBe(8);
    expect(useMetaStore.getState().totalRuns).toBe(3);
  });
});
