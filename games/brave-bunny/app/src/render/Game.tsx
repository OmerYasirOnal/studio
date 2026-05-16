import { Canvas } from '@react-three/fiber';
import { Suspense } from 'react';
import Hero from './Hero';
import CameraRig from './CameraRig';
import Biome from './Biome';
import ProjectileSwarm from './ProjectileSwarm';
import EnemySwarm from './EnemySwarm'; // S7
import PickupSwarm from './PickupSwarm'; // S7/S8
import { RunLoop } from '@/systems/runLoop';

export default function Game() {
  return (
    <Canvas
      camera={{ position: [0, 16, 10], fov: 35 }}
      gl={{ antialias: true, powerPreference: 'high-performance' }}
      dpr={[1, 2]}
      shadows
      data-testid="game-canvas"
    >
      <color attach="background" args={['#bce6ff']} />
      <fog attach="fog" args={['#bce6ff', 20, 60]} />
      <ambientLight intensity={0.7} color="#ffe0c4" />
      <directionalLight
        position={[12, 18, 8]}
        intensity={1.4}
        color="#fff5e0"
        castShadow
        shadow-mapSize-width={1024}
        shadow-mapSize-height={1024}
        shadow-camera-left={-20}
        shadow-camera-right={20}
        shadow-camera-top={20}
        shadow-camera-bottom={-20}
        shadow-camera-near={0.5}
        shadow-camera-far={50}
      />
      <hemisphereLight color="#bce6ff" groundColor="#4a8c3a" intensity={0.5} />
      <Suspense fallback={null}>
        <Biome />
        <Hero />
        <EnemySwarm />
        <PickupSwarm />
      </Suspense>
      <ProjectileSwarm />
      <CameraRig />
      <RunLoop />
    </Canvas>
  );
}
