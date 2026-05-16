type SFXKey = 'hit' | 'enemy-hit' | 'gem' | 'levelup' | 'death' | 'click' | 'draftPick' | 'evolve';

const SFX_FILES: Record<SFXKey, string> = {
  'hit': '/audio/hit.ogg',
  'enemy-hit': '/audio/enemy-hit.ogg',
  'gem': '/audio/gem.ogg',
  'levelup': '/audio/levelup.ogg',
  'death': '/audio/death.ogg',
  'click': '/audio/click.ogg',
  'draftPick': '/audio/draftPick.ogg',
  'evolve': '/audio/evolve.ogg',
};

class AudioBus {
  private ctx: AudioContext | null = null;
  private master: GainNode | null = null;
  private sfxGain: GainNode | null = null;
  private bgmGain: GainNode | null = null;
  private buffers: Partial<Record<SFXKey | 'bgm', AudioBuffer>> = {};
  private bgmSource: AudioBufferSourceNode | null = null;
  private initialized = false;

  async init(): Promise<void> {
    if (this.initialized) return;
    this.ctx = new (window.AudioContext || (window as any).webkitAudioContext)();
    this.master = this.ctx.createGain();
    this.sfxGain = this.ctx.createGain();
    this.bgmGain = this.ctx.createGain();
    this.master.gain.value = 1.0;
    this.sfxGain.gain.value = 0.7;
    this.bgmGain.gain.value = 0.4;
    this.sfxGain.connect(this.master);
    this.bgmGain.connect(this.master);
    this.master.connect(this.ctx.destination);

    const entries = [
      ...Object.entries(SFX_FILES) as [SFXKey, string][],
      ['bgm', '/audio/bgm.ogg'] as ['bgm', string],
    ];
    await Promise.all(entries.map(async ([key, url]) => {
      try {
        const resp = await fetch(url);
        const arrayBuffer = await resp.arrayBuffer();
        const buf = await this.ctx!.decodeAudioData(arrayBuffer);
        this.buffers[key as SFXKey | 'bgm'] = buf;
      } catch (e) {
        console.warn(`AudioBus: failed to load ${key}:`, e);
      }
    }));
    this.initialized = true;
  }

  play(key: SFXKey): void {
    const buf = this.buffers[key];
    if (!buf || !this.ctx || !this.sfxGain) return;
    const src = this.ctx.createBufferSource();
    src.buffer = buf;
    src.connect(this.sfxGain);
    src.start();
  }

  startBgm(): void {
    const buf = this.buffers['bgm'];
    if (!buf || !this.ctx || !this.bgmGain) return;
    if (this.bgmSource) return;
    this.bgmSource = this.ctx.createBufferSource();
    this.bgmSource.buffer = buf;
    this.bgmSource.loop = true;
    this.bgmSource.connect(this.bgmGain);
    this.bgmSource.start();
  }

  stopBgm(): void {
    this.bgmSource?.stop();
    this.bgmSource?.disconnect();
    this.bgmSource = null;
  }

  setSfxVolume(v: number): void {
    if (this.sfxGain) this.sfxGain.gain.value = Math.max(0, Math.min(1, v));
  }
  setBgmVolume(v: number): void {
    if (this.bgmGain) this.bgmGain.gain.value = Math.max(0, Math.min(1, v));
  }
}

export const audio = new AudioBus();
