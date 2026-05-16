import { useFrame } from '@react-three/fiber';
import { useRef } from 'react';
import { Matrix4, Vector3, type InstancedMesh } from 'three';
import { useVfxStore } from '@/state/vfxStore';

const MAX = 60;
const _m = new Matrix4();
const _v = new Vector3();

export default function HitSparks() {
  const ref = useRef<InstancedMesh>(null);
  const sparks = useVfxStore((s) => s.sparks);

  useFrame(() => {
    if (!ref.current) return;
    let i = 0;
    for (const s of sparks) {
      const scale = 1 - s.age / 0.3;
      _v.set(s.x, s.y + 0.5, s.z);
      _m.makeScale(scale * 0.4, scale * 0.4, scale * 0.4);
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
      <octahedronGeometry args={[0.6]} />
      <meshStandardMaterial
        color="#ffba00"
        emissive="#ff6f3c"
        emissiveIntensity={2.2}
        toneMapped={false}
      />
    </instancedMesh>
  );
}
