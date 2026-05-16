import { useRunStore } from '@/state/runStore';
import { world } from '@/ecs/world';
import { useState, useEffect, useRef } from 'react';
import Bar from './Bar';

export default function HUD() {
  const { time, kills, level, xp, xpForNext } = useRunStore();
  const [hp, setHp] = useState({ cur: 100, max: 100 });
  const [flash, setFlash] = useState(false);
  const prevLevelRef = useRef(level);

  useEffect(() => {
    const id = setInterval(() => {
      const hero = world.with('archetype').where((e) => e.archetype === 'hero').first;
      if (hero?.hp != null && hero?.maxHp != null) {
        setHp({ cur: hero.hp, max: hero.maxHp });
      }
    }, 100);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    if (level !== prevLevelRef.current) {
      prevLevelRef.current = level;
      setFlash(true);
      const t = setTimeout(() => setFlash(false), 600);
      return () => clearTimeout(t);
    }
  }, [level]);

  const min = Math.floor(time / 60);
  const sec = Math.floor(time % 60)
    .toString()
    .padStart(2, '0');

  return (
    <>
      <div className="hud-top">
        <div style={{ width: 140 }}>
          <Bar value={hp.cur} max={hp.max} variant="hp" />
          <div className="hud-stat" style={{ marginTop: 6 }}>
            HP {Math.ceil(hp.cur)}/{hp.max}
          </div>
        </div>
        <div className="hud-stat">
          {min}:{sec}
        </div>
        <div className="hud-stat">💀 {kills}</div>
      </div>
      <div className="hud-bottom">
        <div
          className={`hud-stat ${flash ? 'level-flash' : ''}`}
          style={{ marginBottom: 4, display: 'inline-flex' }}
        >
          Lv {level}
        </div>
        <Bar value={xp} max={xpForNext} variant="xp" />
      </div>
    </>
  );
}
