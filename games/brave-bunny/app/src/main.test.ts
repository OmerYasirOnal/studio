import { describe, it, expect } from 'vitest';

describe('app sanity', () => {
  it('arithmetic still works', () => {
    expect(1 + 1).toBe(2);
  });

  it('jsdom provides document', () => {
    expect(typeof document).toBe('object');
    expect(document.createElement).toBeDefined();
  });
});
