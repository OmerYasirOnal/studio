import { describe, it, expect } from 'vitest';
import { isInCone } from './weapons';

const coneCos = Math.cos(Math.PI / 6); // 30° half-angle

describe('isInCone', () => {
  it('hits enemy directly in front', () => {
    expect(isInCone(0, 0, 0, 1, 0, 2, 3, coneCos)).toBe(true);
  });
  it('misses enemy behind hero', () => {
    expect(isInCone(0, 0, 0, 1, 0, -2, 3, coneCos)).toBe(false);
  });
  it('misses enemy 90° to the side', () => {
    expect(isInCone(0, 0, 0, 1, 2, 0, 3, coneCos)).toBe(false);
  });
  it('misses enemy beyond range', () => {
    expect(isInCone(0, 0, 0, 1, 0, 5, 3, coneCos)).toBe(false);
  });
});
