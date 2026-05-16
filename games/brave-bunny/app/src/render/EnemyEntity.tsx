import { useGLTF } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useRef } from 'react';
import type { Group, Mesh, MeshStandardMaterial } from 'three';
import type { Entity } from '@/ecs/components';

const MODEL_MAP: Record<string, string> = {
  slime: '/assets/glb/slime.glb',
  wolf: '/assets/glb/wolf.glb',
  mushroom: '/assets/glb/mushroom.glb',
};

const FLASH_COLOR = '#ffffff';

export default function EnemyEntity({ entity }: { entity: Entity }) {
  const groupRef = useRef<Group>(null);
  const modelKey = entity.archetype as keyof typeof MODEL_MAP;
  const gltf = useGLTF(MODEL_MAP[modelKey] ?? MODEL_MAP.slime);

  useFrame((_, delta) => {
    if (!groupRef.current || !entity.position) return;
    groupRef.current.position.set(entity.position.x, entity.position.y, entity.position.z);
    if (entity.rotationY != null) groupRef.current.rotation.y = entity.rotationY;

    if (entity.hitFlashTime != null && entity.hitFlashTime > 0) {
      entity.hitFlashTime -= delta;
      groupRef.current.traverse((o) => {
        const mesh = o as Mesh;
        if (mesh.isMesh && mesh.material) {
          const mat = mesh.material as MeshStandardMaterial;
          mat.emissive?.set(FLASH_COLOR);
        }
      });
    } else {
      groupRef.current.traverse((o) => {
        const mesh = o as Mesh;
        if (mesh.isMesh && mesh.material) {
          const mat = mesh.material as MeshStandardMaterial;
          mat.emissive?.set('#000000');
        }
      });
    }
  });

  // Each enemy clones the GLTF scene to allow independent positioning.
  // For <=60 enemies on MVP this is acceptable; replace with InstancedMesh + VAT in next plan.
  return (
    <group ref={groupRef}>
      <primitive object={gltf.scene.clone()} />
    </group>
  );
}

useGLTF.preload('/assets/glb/slime.glb');
useGLTF.preload('/assets/glb/wolf.glb');
useGLTF.preload('/assets/glb/mushroom.glb');
