import { useSettingsStore } from '@/state/settingsStore';
import { useMetaStore } from '@/state/metaStore';
import { useEffect } from 'react';
import { audio } from '@/audio/AudioBus';

export default function Settings({ onClose }: { onClose: () => void }) {
  const { bgmVolume, sfxVolume, setBgm, setSfx } = useSettingsStore();
  const resetSave = useMetaStore((s) => s.resetSave);

  useEffect(() => {
    audio.setBgmVolume(bgmVolume);
    audio.setSfxVolume(sfxVolume);
  }, [bgmVolume, sfxVolume]);

  const doReset = () => {
    if (window.confirm('Reset all save data? This cannot be undone.')) {
      resetSave();
      onClose();
    }
  };

  return (
    <div className="overlay overlay--blocking" onClick={onClose}>
      <div className="card" style={{ minWidth: 320 }} onClick={(e) => e.stopPropagation()}>
        <h2 className="title">Settings</h2>
        <label style={{ display: 'block', marginBottom: 16 }}>
          <div style={{ marginBottom: 4 }}>BGM Volume: {Math.round(bgmVolume * 100)}%</div>
          <input
            type="range"
            min={0}
            max={100}
            value={bgmVolume * 100}
            onChange={(e) => setBgm(Number(e.target.value) / 100)}
            style={{ width: '100%' }}
          />
        </label>
        <label style={{ display: 'block', marginBottom: 16 }}>
          <div style={{ marginBottom: 4 }}>SFX Volume: {Math.round(sfxVolume * 100)}%</div>
          <input
            type="range"
            min={0}
            max={100}
            value={sfxVolume * 100}
            onChange={(e) => setSfx(Number(e.target.value) / 100)}
            style={{ width: '100%' }}
          />
        </label>
        <button
          className="btn btn--danger"
          onClick={doReset}
          style={{ width: '100%', marginBottom: 8 }}
        >
          Reset Save
        </button>
        <button className="btn btn--ghost" onClick={onClose} style={{ width: '100%' }}>
          Close
        </button>
      </div>
    </div>
  );
}
