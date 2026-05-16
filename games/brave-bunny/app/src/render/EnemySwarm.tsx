import { useState, useEffect } from 'react';
import { enemyQuery } from '@/ecs/queries';
import EnemyEntity from './EnemyEntity';
import type { Entity } from '@/ecs/components';

export default function EnemySwarm() {
  const [, force] = useState(0);

  useEffect(() => {
    const unsubAdd = enemyQuery.onEntityAdded.subscribe(() => force((n) => n + 1));
    const unsubRemove = enemyQuery.onEntityRemoved.subscribe(() => force((n) => n + 1));
    return () => {
      unsubAdd();
      unsubRemove();
    };
  }, []);

  const enemies: Entity[] = [];
  for (const e of enemyQuery) enemies.push(e);

  return (
    <>
      {enemies.map((e, i) => (
        <EnemyEntity key={(e as unknown as { __id?: number }).__id ?? i} entity={e} />
      ))}
    </>
  );
}
