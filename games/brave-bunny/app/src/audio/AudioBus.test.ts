import { describe, it, expect, beforeAll, vi } from 'vitest';
import { audio } from './AudioBus';

beforeAll(() => {
  (global as unknown as { AudioContext: typeof AudioContext }).AudioContext = class {
    createGain() {
      return { gain: { value: 0 }, connect: vi.fn() };
    }
    createBufferSource() {
      return {
        buffer: null,
        connect: vi.fn(),
        start: vi.fn(),
        stop: vi.fn(),
        disconnect: vi.fn(),
        loop: false,
      };
    }
    decodeAudioData() {
      return Promise.resolve({});
    }
    destination = {};
  } as unknown as typeof AudioContext;
  global.fetch = vi.fn(() =>
    Promise.resolve({ arrayBuffer: () => Promise.resolve(new ArrayBuffer(0)) }),
  ) as unknown as typeof fetch;
});

describe('AudioBus', () => {
  it('initializes without throwing', async () => {
    await expect(audio.init()).resolves.not.toThrow();
  });

  it('play() is a no-op on unloaded sfx', () => {
    expect(() => audio.play('hit')).not.toThrow();
  });
});
