import { describe, it, expect, beforeEach } from 'vitest';
import { useRunStore } from './runStore';
import { useDraftStore } from './draftStore';

describe('XP curve', () => {
  beforeEach(() => useRunStore.getState().reset());

  it('level 1 → level 2 at 23 XP', () => {
    useRunStore.getState().addXp(23);
    expect(useRunStore.getState().level).toBe(2);
  });

  it('level 2 → level 3 at 31 more XP', () => {
    useRunStore.getState().addXp(23); // L1→L2
    useRunStore.getState().addXp(31); // L2→L3
    expect(useRunStore.getState().level).toBe(3);
  });

  it('xpForNext at level 2 is 31', () => {
    useRunStore.getState().addXp(23); // L1→L2 (xpForNext = 15 + 2*8 = 31)
    expect(useRunStore.getState().xpForNext).toBe(31);
  });
});

describe('Draft offers', () => {
  beforeEach(() => useDraftStore.getState().reset());

  it('rollOffers gives at most 3 cards', () => {
    useDraftStore.getState().rollOffers();
    expect(useDraftStore.getState().offers.length).toBeLessThanOrEqual(3);
  });

  it('rollOffers respects maxStacks', () => {
    for (let i = 0; i < 5; i++) {
      useDraftStore.setState((s) => ({ taken: { ...s.taken, hp: i + 1 } }));
    }
    useDraftStore.getState().rollOffers();
    const hasHp = useDraftStore.getState().offers.some((o) => o.kind === 'hp');
    expect(hasHp).toBe(false);
  });
});
