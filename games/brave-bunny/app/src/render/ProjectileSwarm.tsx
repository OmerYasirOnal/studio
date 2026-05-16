import { useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import type { InstancedMesh } from 'three';
import { Matrix4, Vector3 } from 'three';
import { projectileQuery } from '@/ecs/queries';

const MAX_PROJECTILES = 100;
const matrix = new Matrix4();
const tmpPos = new Vector3();

export default function ProjectileSwarm() {
  const meshRef = useRef<InstancedMesh>(null);

  useFrame(() => {
    if (!meshRef.current) return;
    let i = 0;
    for (const p of projectileQuery) {
      if (!p.position) continue;
      tmpPos.set(p.position.x, p.position.y + 0.5, p.position.z);
      matrix.setPosition(tmpPos);
      meshRef.current.setMatrixAt(i, matrix);
      i++;
      if (i >= MAX_PROJECTILES) break;
    }
    meshRef.current.count = i;
    meshRef.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh ref={meshRef} args={[undefined, undefined, MAX_PROJECTILES]}>
      <sphereGeometry args={[0.25, 12, 12]} />
      <meshStandardMaterial color="#ffba00" emissive="#ff6f3c" emissiveIntensity={1.2} />
    </instancedMesh>
  );
}
