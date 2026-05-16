import { useMemo, useRef } from 'react';
import { useFrame } from '@react-three/fiber';
import { Matrix4, Vector3 } from 'three';
import type { InstancedMesh } from 'three';
import { world } from '@/ecs/world';

const TREE_COUNT = 160;
const ROCK_COUNT = 80;
const SCATTER_RADIUS = 45; // around hero
const SCATTER_INNER = 5; // don't spawn within 5u of hero

type DecorGeom = 'tree-trunk' | 'tree-leaves' | 'rock';

function generatePositions(count: number, seed: number): Float32Array {
  const positions = new Float32Array(count * 3);
  // Pseudo-random scatter with golden-ratio spread for even distribution
  const golden = (1 + Math.sqrt(5)) / 2;
  for (let i = 0; i < count; i++) {
    const t = (i + seed * 0.137) / count;
    const angle = i * Math.PI * 2 * (1 - 1 / golden) + seed * 1.7;
    const r = SCATTER_INNER + (SCATTER_RADIUS - SCATTER_INNER) * Math.sqrt(t);
    positions[i * 3] = Math.cos(angle) * r;
    positions[i * 3 + 1] = 0;
    positions[i * 3 + 2] = Math.sin(angle) * r;
  }
  return positions;
}

const _matrix = new Matrix4();
const _pos = new Vector3();

function InstancedDecor({
  count,
  geom,
  scale,
  seed,
}: {
  count: number;
  geom: DecorGeom;
  scale: number;
  seed: number;
}) {
  const meshRef = useRef<InstancedMesh>(null);
  const positions = useMemo(() => generatePositions(count, seed), [count, seed]);

  useFrame(() => {
    if (!meshRef.current) return;
    const hero = world.with('archetype').where((e) => e.archetype === 'hero').first;
    if (!hero?.position) return;

    // Anchor decorations to a coarse grid around hero — repositioning when hero crosses a cell boundary
    const cellSize = SCATTER_RADIUS;
    const gx = Math.floor(hero.position.x / cellSize) * cellSize;
    const gz = Math.floor(hero.position.z / cellSize) * cellSize;

    const yOffset =
      geom === 'tree-leaves' ? scale * 1.4 : geom === 'tree-trunk' ? scale * 0.5 : 0.1;

    for (let i = 0; i < count; i++) {
      const baseX = positions[i * 3];
      const baseZ = positions[i * 3 + 2];
      _pos.set(gx + baseX, yOffset, gz + baseZ);
      _matrix.makeScale(scale, scale, scale);
      _matrix.setPosition(_pos);
      meshRef.current.setMatrixAt(i, _matrix);
    }
    meshRef.current.instanceMatrix.needsUpdate = true;
  });

  return (
    <instancedMesh
      ref={meshRef}
      args={[undefined, undefined, count]}
      castShadow
      receiveShadow
    >
      {geom === 'tree-trunk' && <cylinderGeometry args={[0.15, 0.2, 1.0, 6]} />}
      {geom === 'tree-leaves' && <coneGeometry args={[0.7, 1.6, 6]} />}
      {geom === 'rock' && <dodecahedronGeometry args={[0.4]} />}
      <meshStandardMaterial
        color={
          geom === 'tree-leaves'
            ? '#3e8a3a'
            : geom === 'tree-trunk'
              ? '#6b3a1f'
              : '#888a8c'
        }
        flatShading
      />
    </instancedMesh>
  );
}

export default function Biome() {
  return (
    <>
      {/* Ground plane */}
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, -0.01, 0]} receiveShadow>
        <planeGeometry args={[2000, 2000]} />
        <meshStandardMaterial color="#9be37c" />
      </mesh>

      {/* Hero's local biome ring — slightly darker grass disc */}
      <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, 0, 0]} receiveShadow>
        <circleGeometry args={[35, 32]} />
        <meshStandardMaterial color="#8ad06d" transparent opacity={0.6} />
      </mesh>

      {/* Decorative props (procedural geometry, instanced, follow hero in a coarse grid) */}
      <InstancedDecor count={TREE_COUNT} geom="tree-trunk" scale={1.5} seed={1} />
      <InstancedDecor count={TREE_COUNT} geom="tree-leaves" scale={1.5} seed={1} />
      <InstancedDecor count={ROCK_COUNT} geom="rock" scale={0.8} seed={2} />
    </>
  );
}
