import { describe, it, expect, beforeEach } from 'vitest';
import { useRunStore } from './runStore';
import { useDraftStore } from './draftStore';

describe('XP curve', () => {
  beforeEach(() => useRunStore.getState().reset());

  it('level 1 → level 2 at 10 XP', () => {
    useRunStore.getState().addXp(10);
    expect(useRunStore.getState().level).toBe(2);
  });

  it('xpForNext grows with level', () => {
    useRunStore.getState().addXp(10); // L1→L2 (xpForNext = 10 + 2*5 = 20)
    expect(useRunStore.getState().xpForNext).toBe(20);
  });
});

describe('Draft offers', () => {
  beforeEach(() => useDraftStore.getState().reset());

  it('rollOffers gives at most 3 cards', () => {
    useDraftStore.getState().rollOffers();
    expect(useDraftStore.getState().offers.length).toBeLessThanOrEqual(3);
  });

  it('rollOffers respects maxStacks', () => {
    // Max out HP (5 stacks)
    for (let i = 0; i < 5; i++) {
      useDraftStore.setState((s) => ({ taken: { ...s.taken, hp: i + 1 } }));
    }
    useDraftStore.getState().rollOffers();
    const hasHp = useDraftStore.getState().offers.some((o) => o.kind === 'hp');
    expect(hasHp).toBe(false);
  });
});
