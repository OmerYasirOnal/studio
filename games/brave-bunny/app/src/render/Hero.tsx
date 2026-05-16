import { useGLTF } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useEffect, useRef } from 'react';
import type { Group } from 'three';
import { world } from '@/ecs/world';
import { useRunStore } from '@/state/runStore';

export default function Hero() {
  const groupRef = useRef<Group>(null);
  const gltf = useGLTF('/assets/glb/bunny.glb');
  const input = useRunStore((s) => s.input);

  useEffect(() => {
    const entity = world.add({
      archetype: 'hero',
      position: { x: 0, y: 0, z: 0 },
      velocity: { x: 0, y: 0, z: 0 },
      rotationY: 0,
      hp: 100,
      maxHp: 100,
      team: 'hero',
      speed: 4,
    });
    return () => {
      world.remove(entity);
    };
  }, []);

  useFrame((_, delta) => {
    const hero = world.with('archetype').where((e) => e.archetype === 'hero').first;
    if (!hero || !hero.position) return;

    // Input → velocity
    const speed = hero.speed ?? 4;
    hero.position.x += input.x * speed * delta;
    hero.position.z += input.y * speed * delta;

    if (input.active && (Math.abs(input.x) > 0.05 || Math.abs(input.y) > 0.05)) {
      hero.rotationY = Math.atan2(input.x, input.y);
    }

    if (groupRef.current) {
      groupRef.current.position.set(hero.position.x, hero.position.y, hero.position.z);
      groupRef.current.rotation.y = hero.rotationY ?? 0;
    }
  });

  return (
    <group ref={groupRef}>
      <primitive object={gltf.scene} scale={1} />
    </group>
  );
}

useGLTF.preload('/assets/glb/bunny.glb');
