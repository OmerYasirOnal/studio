#nullable enable
// Brave Bunny — Gameplay / MVP / MvpWaveSpawner
//
// Wave 13 vertical-slice spawner. Periodically (every <see cref="spawnIntervalSeconds"/>)
// instantiates <see cref="swarmersPerWave"/> MvpSwarmer GameObjects on a ring around
// the hero at radius <see cref="ringRadius"/>. Used while the real WaveDefinition SO
// + EnemyDefinition.prefab pipeline is being wired (see hand-off doc).
//
// Why not the canonical WaveSpawner: WaveSpawner consumes a WaveDefinition SO whose
// entries reference EnemyDefinitions, each pointing at an enemy prefab. Wave 13 has
// neither the SO nor the prefabs authored (Assets/_Brave contains zero .prefab files).
// This MVP component sidesteps the asset chain so the Run scene is playable today,
// then yields gracefully when a real WaveSpawner is wired in a follow-up wave.
//
// Allocation discipline: builds the enemy GameObjects via Instantiate on spawn
// (Wave 13 cap is 50 active enemies — within the GC budget). The pooled path is
// reserved for the canonical EnemyBase-based spawner.

using UnityEngine;

namespace Brave.Gameplay.MVP
{
    /// <summary>
    /// Time-driven swarm spawner. Spawns on a fixed cadence around the hero.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MvpWaveSpawner : MonoBehaviour
    {
        [Header("Spawn cadence")]
        [Tooltip("Seconds between spawn pulses. 3s = the Wave-13 brief's test cadence.")]
        [SerializeField, Min(0.05f)] private float spawnIntervalSeconds = 3f;

        [Tooltip("Enemies per pulse. 5 = the Wave-13 brief's test count.")]
        [SerializeField, Min(1)] private int swarmersPerWave = 5;

        [Header("Spawn placement")]
        [Tooltip("World-space radius around the hero where enemies appear.")]
        [SerializeField, Min(0.1f)] private float ringRadius = 8f;

        [Tooltip("Maximum simultaneously-active swarmers. Spawner skips pulses above this.")]
        [SerializeField, Min(1)] private int activeCap = 50;

        [Header("Wiring")]
        [SerializeField] private Transform? hero;
        [Tooltip("Enemy prefab to instantiate. If null, MvpWaveSpawner builds a default cube swarmer at runtime.")]
        [SerializeField] private GameObject? swarmerPrefab;

        [Header("Swarmer tuning")]
        [SerializeField, Min(0.1f)] private float swarmerMoveSpeed = 2.5f;
        [SerializeField, Min(1f)] private float swarmerHp = 10f;

        private float _accum;
        private int _activeCount;
        private Material? _sharedSwarmerMaterial;

        public Transform? Hero { get => hero; set => hero = value; }

        public void Configure(Transform target, float intervalSec, int countPerWave, float radius, int cap)
        {
            hero = target;
            spawnIntervalSeconds = Mathf.Max(0.05f, intervalSec);
            swarmersPerWave = Mathf.Max(1, countPerWave);
            ringRadius = Mathf.Max(0.1f, radius);
            activeCap = Mathf.Max(1, cap);
        }

        private void Update()
        {
            if (hero == null) return;

            _accum += Time.deltaTime;
            if (_accum < spawnIntervalSeconds) return;
            _accum -= spawnIntervalSeconds;

            if (_activeCount >= activeCap) return;

            int toSpawn = swarmersPerWave;
            // Cap the burst to leave room under activeCap.
            if (_activeCount + toSpawn > activeCap) toSpawn = activeCap - _activeCount;

            Vector3 origin = hero.position;
            float twoPi = Mathf.PI * 2f;
            for (int i = 0; i < toSpawn; i++)
            {
                // Random angle on the ring — keeps spawn pattern surprising without
                // pulling in the proper SpawnerRing helper (which expects pooled enemies).
                float angle = Random.value * twoPi;
                Vector3 pos = origin + new Vector3(Mathf.Cos(angle) * ringRadius, 0f, Mathf.Sin(angle) * ringRadius);
                SpawnOne(pos);
                _activeCount++;
            }
        }

        private void SpawnOne(Vector3 worldPos)
        {
            GameObject go;
            if (swarmerPrefab != null)
            {
                go = Instantiate(swarmerPrefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                go = BuildDefaultSwarmerGo(worldPos);
            }

            var swarmer = go.GetComponent<MvpSwarmer>();
            if (swarmer == null) swarmer = go.AddComponent<MvpSwarmer>();
            swarmer.ConfigureMvp(hero!, swarmerMoveSpeed, swarmerHp);
        }

        private GameObject BuildDefaultSwarmerGo(Vector3 worldPos)
        {
            // Default cube swarmer — onion-purple URP/Lit material per art-bible Meadow.
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MvpSwarmer";
            go.transform.SetParent(transform, worldPositionStays: false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);

            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (_sharedSwarmerMaterial == null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) shader = Shader.Find("Standard");
                    _sharedSwarmerMaterial = new Material(shader) { color = new Color(0.55f, 0.3f, 0.6f) };
                }
                renderer.sharedMaterial = _sharedSwarmerMaterial;
            }
            return go;
        }
    }
}
