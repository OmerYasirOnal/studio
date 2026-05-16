/* global HTMLDivElement, PointerEvent */
import { useRef, useEffect } from 'react';
import { useRunStore } from '@/state/runStore';
import './joystick.css';

export default function Joystick() {
  const ref = useRef<HTMLDivElement>(null);
  const knobRef = useRef<HTMLDivElement>(null);
  const setInput = useRunStore((s) => s.setInput);

  useEffect(() => {
    const el = ref.current!;
    let origin: { x: number; y: number } | null = null;

    const onDown = (e: PointerEvent) => {
      const rect = el.getBoundingClientRect();
      origin = { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
      el.setPointerCapture(e.pointerId);
      onMove(e);
    };

    const onMove = (e: PointerEvent) => {
      if (!origin) return;
      const dx = e.clientX - origin.x;
      const dy = e.clientY - origin.y;
      const dist = Math.hypot(dx, dy);
      const maxDist = 50;
      const clampedDist = Math.min(dist, maxDist);
      const nx = dist === 0 ? 0 : (dx / dist) * (clampedDist / maxDist);
      const ny = dist === 0 ? 0 : (dy / dist) * (clampedDist / maxDist);
      setInput({ x: nx, y: ny, active: true });
      if (knobRef.current) {
        const knobOffset = clampedDist;
        knobRef.current.style.transform = `translate(${(dx / dist || 0) * knobOffset}px, ${(dy / dist || 0) * knobOffset}px)`;
      }
    };

    const onUp = (e: PointerEvent) => {
      origin = null;
      setInput({ x: 0, y: 0, active: false });
      el.releasePointerCapture(e.pointerId);
      if (knobRef.current) knobRef.current.style.transform = '';
    };

    el.addEventListener('pointerdown', onDown);
    el.addEventListener('pointermove', onMove);
    el.addEventListener('pointerup', onUp);
    el.addEventListener('pointercancel', onUp);

    return () => {
      el.removeEventListener('pointerdown', onDown);
      el.removeEventListener('pointermove', onMove);
      el.removeEventListener('pointerup', onUp);
      el.removeEventListener('pointercancel', onUp);
    };
  }, [setInput]);

  return (
    <div ref={ref} className="joystick" data-testid="joystick">
      <div ref={knobRef} className="joystick__knob" />
    </div>
  );
}
