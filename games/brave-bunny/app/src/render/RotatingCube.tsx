import { useFrame } from '@react-three/fiber';
import { useRef } from 'react';
import { type Mesh } from 'three';

export default function RotatingCube() {
  const ref = useRef<Mesh>(null);

  useFrame((_, delta) => {
    if (!ref.current) return;
    ref.current.rotation.x += delta * 0.5;
    ref.current.rotation.y += delta * 0.8;
  });

  return (
    <mesh ref={ref} position={[0, 0.5, 0]}>
      <boxGeometry args={[1, 1, 1]} />
      <meshStandardMaterial color="#ff6f3c" />
    </mesh>
  );
}
