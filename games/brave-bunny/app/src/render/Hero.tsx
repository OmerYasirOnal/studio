import { useGLTF, useAnimations } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useEffect, useRef } from 'react';
import type { Group } from 'three';
import { world } from '@/ecs/world';
import { useRunStore } from '@/state/runStore';
import { setMagnetRadius } from '@/systems/lifecycle';

export default function Hero() {
  const groupRef = useRef<Group>(null);
  const gltf = useGLTF('/assets/glb/bunny.glb');
  const { actions, names } = useAnimations(gltf.animations, groupRef);
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

  // Resolve animation names by suffix (Quaternius prefixes vary:
  // CharacterArmature|, AnimalArmature|, MonsterArmature|).
  const findAnim = (suffix: string): string | undefined =>
    names.find((n) => n.endsWith(suffix)) ?? names[0];
  const IDLE = findAnim('Idle');
  const RUN = findAnim('Run') ?? IDLE;

  // Play idle by default on mount
  useEffect(() => {
    if (!actions || !IDLE) return;
    const action = actions[IDLE];
    action?.reset().fadeIn(0.2).play();
    return () => {
      action?.fadeOut(0.2);
    };
  }, [actions, IDLE]);

  // Crossfade between idle and run based on input.active
  const currentAnimRef = useRef<string | undefined>(IDLE);

  useFrame((_, delta) => {
    const target =
      input.active && (Math.abs(input.x) > 0.1 || Math.abs(input.y) > 0.1) ? RUN : IDLE;
    if (
      target &&
      currentAnimRef.current &&
      currentAnimRef.current !== target &&
      actions[target] &&
      actions[currentAnimRef.current]
    ) {
      actions[currentAnimRef.current]?.fadeOut(0.15);
      actions[target]?.reset().fadeIn(0.15).play();
      currentAnimRef.current = target;
    }

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
      <primitive object={gltf.scene} scale={0.5} />
    </group>
  );
}

useGLTF.preload('/assets/glb/bunny.glb');
