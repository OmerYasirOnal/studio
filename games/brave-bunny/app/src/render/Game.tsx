import { Canvas } from '@react-three/fiber';
import RotatingCube from './RotatingCube';

export default function Game() {
  return (
    <Canvas
      camera={{ position: [0, 2, 5], fov: 35 }}
      gl={{ antialias: true, powerPreference: 'high-performance' }}
      dpr={[1, 2]}
      data-testid="game-canvas"
    >
      <ambientLight intensity={0.6} />
      <directionalLight position={[5, 5, 5]} intensity={1.0} />
      <RotatingCube />
    </Canvas>
  );
}
