import { useGLTF } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useEffect, useMemo, useRef } from 'react';
import { AnimationMixer, type Group, type Mesh, type MeshStandardMaterial } from 'three';
import { SkeletonUtils } from 'three-stdlib';
import type { Entity } from '@/ecs/components';

const MODEL_MAP: Record<string, string> = {
  slime: '/assets/glb/slime.glb',
  wolf: '/assets/glb/wolf.glb',
  mushroom: '/assets/glb/mushroom.glb',
};

const SCALE_MAP: Record<string, number> = {
  slime: 0.6,
  wolf: 0.7,
  mushroom: 0.5,
};

export default function EnemyEntity({ entity }: { entity: Entity }) {
  const groupRef = useRef<Group>(null);
  const modelKey = entity.archetype as keyof typeof MODEL_MAP;
  const gltf = useGLTF(MODEL_MAP[modelKey] ?? MODEL_MAP.slime);
  const scale = SCALE_MAP[modelKey] ?? 0.5;

  // Per-instance scene clone + mixer (SkeletonUtils.clone preserves skeleton).
  const clonedScene = useMemo(() => SkeletonUtils.clone(gltf.scene), [gltf.scene]);
  const mixer = useMemo(() => new AnimationMixer(clonedScene), [clonedScene]);

  // Pick walk/idle animation by suffix and play it on a loop.
  useEffect(() => {
    const clip =
      gltf.animations.find((a) => a.name.endsWith('Walk')) ??
      gltf.animations.find((a) => a.name.endsWith('Idle')) ??
      gltf.animations[0];
    if (!clip) return;
    const action = mixer.clipAction(clip);
    action.reset().fadeIn(0.1).play();
    return () => {
      action.fadeOut(0.1);
      mixer.stopAllAction();
    };
  }, [mixer, gltf.animations]);

  useFrame((_, delta) => {
    mixer.update(delta);
    if (!groupRef.current || !entity.position) return;
    groupRef.current.position.set(entity.position.x, entity.position.y, entity.position.z);
    if (entity.rotationY != null) groupRef.current.rotation.y = entity.rotationY;

    if (entity.hitFlashTime != null && entity.hitFlashTime > 0) {
      entity.hitFlashTime -= delta;
      clonedScene.traverse((o) => {
        const mesh = o as Mesh;
        if (mesh.isMesh && mesh.material) {
          const mat = mesh.material as MeshStandardMaterial;
          if (mat.emissive) mat.emissive.setHex(0xffffff);
        }
      });
    } else {
      clonedScene.traverse((o) => {
        const mesh = o as Mesh;
        if (mesh.isMesh && mesh.material) {
          const mat = mesh.material as MeshStandardMaterial;
          if (mat.emissive) mat.emissive.setHex(0x000000);
        }
      });
    }
  });

  return (
    <group ref={groupRef} scale={scale}>
      <primitive object={clonedScene} />
    </group>
  );
}

useGLTF.preload('/assets/glb/slime.glb');
useGLTF.preload('/assets/glb/wolf.glb');
useGLTF.preload('/assets/glb/mushroom.glb');
