import { useFrame } from '@react-three/fiber';
import { useRef } from 'react';
import { Matrix4, Vector3, type InstancedMesh } from 'three';
import { useVfxStore } from '@/state/vfxStore';

const MAX = 30;
const _m = new Matrix4();
const _v = new Vector3();

export default function DeathPoofs() {
  const ref = useRef<InstancedMesh>(null);
  const poofs = useVfxStore((s) => s.poofs);

  useFrame(() => {
    if (!ref.current) return;
    let i = 0;
    for (const p of poofs) {
      const t = p.age / 0.4;
      const radius = 0.3 + t * 1.0;
      _v.set(p.x, p.y + 0.4 + t * 0.6, p.z);
      _m.makeScale(radius, radius, radius);
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
      <sphereGeometry args={[1, 8, 8]} />
      <meshStandardMaterial color="#cccccc" transparent opacity={0.5} />
    </instancedMesh>
  );
}
