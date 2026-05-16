import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import { Matrix4, Vector3, type InstancedMesh } from 'three';
import { pickupQuery } from '@/ecs/queries';

const MAX_PICKUPS = 200;
const matrix = new Matrix4();
const pos = new Vector3();

export default function PickupSwarm() {
  const meshRef = useRef<InstancedMesh>(null);
  const t = useRef(0);

  useFrame((_, delta) => {
    if (!meshRef.current) return;
    t.current += delta;
    let i = 0;
    for (const p of pickupQuery) {
      if (!p.position) continue;
      pos.set(p.position.x, p.position.y + 0.4 + Math.sin(t.current * 4 + i) * 0.1, p.position.z);
      matrix.setPosition(pos);
      meshRef.current.setMatrixAt(i, matrix);
      i++;
      if (i >= MAX_PICKUPS) break;
    }
    meshRef.current.count = i;
    meshRef.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh ref={meshRef} args={[undefined, undefined, MAX_PICKUPS]}>
      <octahedronGeometry args={[0.2]} />
      <meshStandardMaterial color="#5acff6" emissive="#5acff6" emissiveIntensity={0.6} />
    </instancedMesh>
  );
}
