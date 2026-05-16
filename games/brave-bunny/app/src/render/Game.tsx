import { Canvas } from '@react-three/fiber';
import { Suspense } from 'react';
import Hero from './Hero';
import CameraRig from './CameraRig';
import Biome from './Biome';

export default function Game() {
  return (
    <Canvas
      camera={{ position: [0, 14, 9], fov: 35 }}
      gl={{ antialias: true, powerPreference: 'high-performance' }}
      dpr={[1, 2]}
      shadows
      data-testid="game-canvas"
    >
      <ambientLight intensity={0.6} />
      <directionalLight position={[10, 14, 8]} intensity={1.2} castShadow />
      <Suspense fallback={null}>
        <Biome />
        <Hero />
      </Suspense>
      <CameraRig />
    </Canvas>
  );
}
