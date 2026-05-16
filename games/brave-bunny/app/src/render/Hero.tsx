import { useGLTF } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useEffect, useRef } from 'react';
import type { Group } from 'three';
import { world } from '@/ecs/world';
import { useRunStore } from '@/state/runStore';
import { setMagnetRadius } from '@/systems/lifecycle';

export default function Hero() {
  const groupRef = useRef<Group>(null);
  const gltf = useGLTF('/assets/glb/bunny.glb');
  const input = useRunStore((s) => s.input);
  const phase = useRunStore((s) => s.phase);

  useEffect(() => {
    // Mount once on lobby→run, remove on endrun→lobby
    if (phase === 'run') {
      // Check if hero already exists
      const existing = world.with('archetype').where((e) => e.archetype === 'hero').first;
      if (existing) return; // already mounted

      world.add({
        archetype: 'hero',
        position: { x: 0, y: 0, z: 0 },
        velocity: { x: 0, y: 0, z: 0 },
        rotationY: 0,
        hp: 100,
        maxHp: 100,
        team: 'hero',
        speed: 4,
        weapons: [
          { kind: 'spear', damage: 15, tickInterval: 0.6, cooldown: 0, level: 1 },
          { kind: 'sling', damage: 10, tickInterval: 0.4, cooldown: 0, level: 1 },
        ],
      });
    } else if (phase === 'lobby') {
      // Clean up all entities
      for (const e of world.entities) world.remove(e);
      setMagnetRadius(2);
    }
  }, [phase]);

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
