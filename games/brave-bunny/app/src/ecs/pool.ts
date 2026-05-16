export interface Pooled {
  active: boolean;
}

export interface Pool<T extends Pooled> {
  acquire(): T;
  release(item: T): void;
  forEachActive(fn: (item: T) => void): void;
  countActive(): number;
}

export function createPool<T extends Pooled>(factory: () => T, initialSize: number): Pool<T> {
  const items: T[] = [];
  for (let i = 0; i < initialSize; i++) items.push(factory());

  return {
    acquire(): T {
      for (const item of items) {
        if (!item.active) {
          item.active = true;
          return item;
        }
      }
      const fresh = factory();
      fresh.active = true;
      items.push(fresh);
      return fresh;
    },
    release(item: T): void {
      item.active = false;
    },
    forEachActive(fn): void {
      for (const item of items) if (item.active) fn(item);
    },
    countActive(): number {
      let n = 0;
      for (const item of items) if (item.active) n++;
      return n;
    },
  };
}
