import { useFrame } from '@react-three/fiber';
import { useRef } from 'react';
import { Matrix4, Vector3, type InstancedMesh } from 'three';
import { useVfxStore } from '@/state/vfxStore';

const MAX = 8;
const _m = new Matrix4();
const _v = new Vector3();

export default function LevelUpBurst() {
  const ref = useRef<InstancedMesh>(null);
  const bursts = useVfxStore((s) => s.bursts);

  useFrame(() => {
    if (!ref.current) return;
    let i = 0;
    for (const b of bursts) {
      const t = b.age / 0.6;
      const radius = 0.5 + t * 4;
      _v.set(b.x, b.y + 0.3, b.z);
      // Scale a torus geometry: x/z = radius, y = thickness factor
      _m.makeScale(radius, 1 + t * 0.6, radius);
      _m.setPosition(_v);
      ref.current.setMatrixAt(i, _m);
      i++;
      if (i >= MAX) break;
    }
    ref.current.count = i;
    ref.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh ref={ref} args={[undefined, undefined, MAX]}>
      <torusGeometry args={[1, 0.08, 8, 24]} />
      <meshStandardMaterial
        color="#ffba00"
        emissive="#ff6f3c"
        emissiveIntensity={1.6}
        transparent
        opacity={0.6}
        toneMapped={false}
      />
    </instancedMesh>
  );
}
