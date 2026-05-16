import { useGLTF, useAnimations } from '@react-three/drei';
import { useFrame } from '@react-three/fiber';
import { useEffect, useRef } from 'react';
import { LoopOnce, LoopRepeat, type Group } from 'three';
import { world } from '@/ecs/world';
import { useRunStore } from '@/state/runStore';
import { setMagnetRadius } from '@/systems/lifecycle';

type AnimState = 'idle' | 'run' | 'attack' | 'hit' | 'death';

export default function Hero() {
  const groupRef = useRef<Group>(null);
  const gltf = useGLTF('/assets/glb/bunny.glb');
  const { actions, names } = useAnimations(gltf.animations, groupRef);
  const input = useRunStore((s) => s.input);
  const phase = useRunStore((s) => s.phase);

  useEffect(() => {
    // Mount once on lobby→run, remove on endrun→lobby
    if (phase === 'run') {
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
  const PUNCH = findAnim('Punch') ?? IDLE;
  const HITREACT = findAnim('HitReact') ?? IDLE;
  const DEATH = findAnim('Death') ?? IDLE;

  const stateRef = useRef<AnimState>('idle');
  const overrideRemainingRef = useRef<number>(0); // seconds remaining on attack/hit override
  const lastHpRef = useRef<number>(100);
  const lastCooldownRef = useRef<number[]>([0, 0]);

  // Play idle by default on mount
  useEffect(() => {
    if (!actions || !IDLE) return;
    const action = actions[IDLE];
    action?.reset().fadeIn(0.2).play();
    stateRef.current = 'idle';
    return () => {
      action?.fadeOut(0.2);
    };
  }, [actions, IDLE]);

  const playState = (next: AnimState, overrideSeconds?: number): void => {
    if (stateRef.current === next) return;
    const mapping: Record<AnimState, string | undefined> = {
      idle: IDLE,
      run: RUN,
      attack: PUNCH,
      hit: HITREACT,
      death: DEATH,
    };
    const fromName = mapping[stateRef.current];
    const toName = mapping[next];
    if (!toName) return;
    const fromAction = fromName ? actions[fromName] : null;
    const toAction = actions[toName];
    if (!toAction) return;
    fromAction?.fadeOut(0.12);
    toAction.reset();
    if (next === 'death') {
      toAction.setLoop(LoopOnce, 1);
      toAction.clampWhenFinished = true;
    } else if (next === 'attack' || next === 'hit') {
      toAction.setLoop(LoopOnce, 1);
      toAction.clampWhenFinished = false;
    } else {
      toAction.setLoop(LoopRepeat, Infinity);
      toAction.clampWhenFinished = false;
    }
    toAction.fadeIn(0.12).play();
    stateRef.current = next;
    if (overrideSeconds != null) {
      overrideRemainingRef.current = overrideSeconds;
    }
  };

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

    // Animation state machine — priority: death > hit > attack > run > idle
    const hp = hero.hp ?? 0;
    if (overrideRemainingRef.current > 0) overrideRemainingRef.current -= delta;
    const overrideActive = overrideRemainingRef.current > 0;

    // Death takes precedence — one-shot, hold last frame
    if (hp <= 0 && stateRef.current !== 'death') {
      playState('death');
      return;
    }
    if (stateRef.current === 'death') return; // hold death

    // Hit react if HP dropped this frame
    if (hp < lastHpRef.current && hp > 0) {
      lastHpRef.current = hp;
      playState('hit', 0.28);
      return;
    }
    lastHpRef.current = hp;

    // Attack flash when weapon just fired (cooldown jumped from ~0 to ≥tickInterval/2)
    if (hero.weapons) {
      for (let i = 0; i < hero.weapons.length; i++) {
        const w = hero.weapons[i];
        const prev = lastCooldownRef.current[i] ?? 0;
        if (prev < 0.1 && w.cooldown > w.tickInterval * 0.5) {
          if (!overrideActive) {
            playState('attack', 0.35);
            lastCooldownRef.current[i] = w.cooldown;
            return;
          }
        }
        lastCooldownRef.current[i] = w.cooldown;
      }
    }

    // Default: idle ⇄ run (only when no override active)
    if (!overrideActive) {
      const moving = input.active && (Math.abs(input.x) > 0.1 || Math.abs(input.y) > 0.1);
      playState(moving ? 'run' : 'idle');
    }
  });

  return (
    <group ref={groupRef}>
      <primitive object={gltf.scene} scale={0.5} />
    </group>
  );
}

useGLTF.preload('/assets/glb/bunny.glb');
