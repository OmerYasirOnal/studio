import { describe, it, expect } from 'vitest';
import { createPool } from './pool';

describe('createPool', () => {
  it('reuses dead entities', () => {
    const pool = createPool(() => ({ active: false, value: 0 }), 5);
    const a = pool.acquire();
    a.value = 42;
    pool.release(a);
    const b = pool.acquire();
    expect(b).toBe(a);
    expect(b.active).toBe(true);
  });

  it('grows past initial size if exhausted', () => {
    const pool = createPool(() => ({ active: false }), 2);
    pool.acquire();
    pool.acquire();
    const third = pool.acquire();
    expect(third.active).toBe(true);
  });
});
