import { useThree, useFrame } from '@react-three/fiber';
import { Vector3 } from 'three';
import { world } from '@/ecs/world';

const OFFSET = new Vector3(0, 14, 9);
const TARGET = new Vector3();
const DESIRED = new Vector3();

export default function CameraRig() {
  const camera = useThree((s) => s.camera);

  useFrame((_, delta) => {
    const hero = world.with('archetype').where((e) => e.archetype === 'hero').first;
    if (!hero?.position) return;
    TARGET.set(hero.position.x, hero.position.y, hero.position.z);
    DESIRED.copy(TARGET).add(OFFSET);
    camera.position.lerp(DESIRED, 1 - Math.exp(-delta / 0.15));
    camera.lookAt(TARGET);
  });

  return null;
}
