import { useThree, useFrame } from '@react-three/fiber';
import { useVfxStore } from '@/state/vfxStore';
import { Vector3 } from 'three';
import { useEffect, useRef, useState } from 'react';

interface ScreenDamage {
  id: number;
  amount: number;
  sx: number;
  sy: number;
  opacity: number;
}

// Module-level shared buffer + subscribers — written by the in-canvas projector,
// consumed by the DOM overlay component mounted outside the Canvas.
let _frame: ScreenDamage[] = [];
const _subs = new Set<(snap: ScreenDamage[]) => void>();
function _publish(next: ScreenDamage[]): void {
  _frame = next;
  for (const fn of _subs) fn(next);
}

const tmpVec = new Vector3();

/**
 * Lives inside the R3F Canvas tree. Projects world-space damage events
 * to screen-space and publishes to subscribers (the DOM overlay).
 * Returns null so it contributes nothing to the THREE scene graph.
 */
export function DamageNumbersProjector(): null {
  const damages = useVfxStore((s) => s.damages);
  const camera = useThree((s) => s.camera);
  const gl = useThree((s) => s.gl);

  useFrame(() => {
    const w = gl.domElement.clientWidth;
    const h = gl.domElement.clientHeight;
    const next: ScreenDamage[] = new Array(damages.length);
    for (let i = 0; i < damages.length; i++) {
      const d = damages[i];
      tmpVec.set(d.x, d.y + 1.5 + d.age * 1.2, d.z);
      tmpVec.project(camera);
      next[i] = {
        id: d.id,
        amount: d.amount,
        sx: (tmpVec.x * 0.5 + 0.5) * w,
        sy: (1 - (tmpVec.y * 0.5 + 0.5)) * h,
        opacity: Math.max(0, 1 - d.age / 0.8),
      };
    }
    _publish(next);
  });

  return null;
}

/**
 * Lives outside the R3F Canvas tree. Renders the floating damage number DOM.
 * Subscribes to the projector's published screen positions and re-renders
 * on each frame the projector publishes.
 */
export default function DamageNumbers() {
  const [snap, setSnap] = useState<ScreenDamage[]>(_frame);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const fn = (s: ScreenDamage[]): void => setSnap(s);
    _subs.add(fn);
    return () => {
      _subs.delete(fn);
    };
  }, []);

  return (
    <div
      ref={containerRef}
      style={{
        position: 'fixed',
        inset: 0,
        pointerEvents: 'none',
        zIndex: 50,
      }}
    >
      {snap.map((d) => (
        <div
          key={d.id}
          style={{
            position: 'absolute',
            left: 0,
            top: 0,
            transform: `translate(${d.sx}px, ${d.sy}px) translate(-50%, -50%)`,
            opacity: d.opacity,
            color: d.amount > 20 ? '#ff6f3c' : '#ffba00',
            fontSize: d.amount > 20 ? 28 : 20,
            fontWeight: 800,
            textShadow: '0 0 4px rgba(0,0,0,0.8), 0 2px 4px rgba(0,0,0,0.6)',
            whiteSpace: 'nowrap',
            willChange: 'transform, opacity',
          }}
        >
          {d.amount}
        </div>
      ))}
    </div>
  );
}
