#nullable enable
// Brave Bunny — Hit Feedback Juice
// Spawns pooled, world-space floating damage numbers. Each widget is a TMP_Text
// instance (TextMeshPro is in Packages/manifest.json — com.unity.textmeshpro 3.2.0-pre.10).
//
// Per-hit flow:
//   1. Spawner.Spawn(in HitContext) acquires a free widget from DamageNumberPool.
//   2. Widget positions at hit point + small jitter; sets color (normal / crit / player).
//   3. Widget tweens upward + alpha-fade over FeelConfig.dmgNumberLifetime.
//   4. On expiry, Widget calls back into the pool to release itself.
//
// Zero allocations on the hot path: pre-warmed pool; per-spawn state is set via
// fields on the widget, no GC. Number-to-string formatting uses TMP's
// SetCharArray to avoid string allocations.

using TMPro;
using UnityEngine;

using Brave.Gameplay.Damage;
using Brave.Gameplay.Pooling;

namespace Brave.Gameplay.Feel
{
    /// <summary>Kind selector for damage-number color. </summary>
    public enum DamageNumberKind
    {
        Normal     = 0,
        Crit       = 1,
        PlayerHurt = 2,
    }

    /// <summary>
    /// Pool-friendly TMP widget used by <see cref="DamageNumberSpawner"/>. Holds the
    /// per-instance animation state and ticks itself from Update. Returned to its owning
    /// pool when the lifetime expires.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageNumberWidget : MonoBehaviour, IPoolable
    {
        [SerializeField] private TMP_Text? _label;

        private DamageNumberPool? _owner;
        private float _spawnUnscaledTime;
        private float _lifetimeSeconds;
        private float _floatHeight;
        private Vector3 _origin;
        private Color _startColor = Color.white;
        private bool _active;

        // Pre-sized char buffer used by SetCharArray. 8 chars covers up to 9_999_999 dmg.
        private readonly char[] _chars = new char[8];

        public bool IsActive => _active;

        /// <summary>Test hook: owning pool reference (set by spawner).</summary>
        public DamageNumberPool? Owner { get => _owner; set => _owner = value; }

        public TMP_Text? Label { get => _label; set => _label = value; }

        public void OnGetFromPool() { /* per-spawn state set in Configure() */ }

        public void OnReturnToPool()
        {
            _active = false;
            if (_label != null) _label.text = string.Empty;
        }

        /// <summary>Configure for a new floating number. Called by the spawner right after Acquire().</summary>
        public void Configure(
            Vector3 worldPos,
            float amount,
            Color color,
            float lifetimeSeconds,
            float floatHeight,
            float unscaledNow)
        {
            transform.position = worldPos;
            _origin = worldPos;
            _spawnUnscaledTime = unscaledNow;
            _lifetimeSeconds = Mathf.Max(0.01f, lifetimeSeconds);
            _floatHeight = floatHeight;
            _startColor = color;
            _active = true;

            if (_label != null)
            {
                _label.color = color;
                WriteIntTo(_chars, Mathf.Max(0, Mathf.RoundToInt(amount)), out int written);
                _label.SetCharArray(_chars, 0, written);
            }
        }

        private void Update()
        {
            if (!_active) return;
            Tick(Time.unscaledTime);
        }

        /// <summary>Pure tick — exposed for EditMode tests so we don't depend on Unity's loop.</summary>
        public void Tick(float unscaledNow)
        {
            if (!_active) return;

            float t = (unscaledNow - _spawnUnscaledTime) / _lifetimeSeconds;
            if (t >= 1f)
            {
                _active = false;
                _owner?.Release(this);
                return;
            }

            // Float upward; ease-out via 1 - (1-t)^2 keeps the rise snappy.
            float easedRise = 1f - (1f - t) * (1f - t);
            var p = _origin;
            p.y += _floatHeight * easedRise;
            transform.position = p;

            // Linear alpha fade to zero across the lifetime.
            if (_label != null)
            {
                var c = _startColor;
                c.a = 1f - t;
                _label.color = c;
            }
        }

        /// <summary>Format <paramref name="value"/> as decimal digits into <paramref name="buffer"/>. Zero allocations.</summary>
        internal static void WriteIntTo(char[] buffer, int value, out int written)
        {
            if (value == 0)
            {
                buffer[0] = '0';
                written = 1;
                return;
            }
            int v = value;
            int len = 0;
            int max = buffer.Length;
            while (v > 0 && len < max)
            {
                buffer[len++] = (char)('0' + (v % 10));
                v /= 10;
            }
            // Reverse in-place.
            for (int i = 0, j = len - 1; i < j; i++, j--)
            {
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }
            written = len;
        }
    }

    /// <summary>
    /// Spawns floating damage numbers at hit positions. Drives the pool, picks the
    /// color (normal / crit / player-hurt), and forwards lifetime + jitter from
    /// <see cref="FeelConfig"/>. Allocation-free in the hot path.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private FeelConfig? _config;
        [SerializeField] private DamageNumberPool? _pool;

        // Deterministic-ish jitter source (rotates each spawn; no Random alloc).
        private int _jitterCursor;
        private static readonly Vector2[] _jitterTable =
        {
            new(-1f,  0f), new(1f, 0f), new(0f, 1f), new(0f, -1f),
            new(-0.7f, 0.7f), new(0.7f, -0.7f), new(-0.7f, -0.7f), new(0.7f, 0.7f),
        };

        public FeelConfig? Config { get => _config; set => _config = value; }
        public DamageNumberPool? Pool { get => _pool; set => _pool = value; }

        /// <summary>Spawn from a HitContext (preferred — has crit flag + position).</summary>
        public DamageNumberWidget? Spawn(in HitContext ctx)
        {
            var kind = ctx.isCrit ? DamageNumberKind.Crit : DamageNumberKind.Normal;
            return Spawn(ctx.hitPoint, ctx.amount, kind);
        }

        /// <summary>Spawn from a HitInfo (used by EnemyHealth call site).</summary>
        public DamageNumberWidget? Spawn(in HitInfo info)
        {
            var kind = info.isCrit ? DamageNumberKind.Crit : DamageNumberKind.Normal;
            return Spawn(info.impactPosition, info.amount, kind);
        }

        /// <summary>Spawn for player-hurt feedback (red).</summary>
        public DamageNumberWidget? SpawnPlayerHurt(Vector3 worldPos, float amount)
            => Spawn(worldPos, amount, DamageNumberKind.PlayerHurt);

        /// <summary>Core spawn path. Returns the acquired widget, or null when pool is empty.</summary>
        public DamageNumberWidget? Spawn(Vector3 worldPos, float amount, DamageNumberKind kind)
        {
            if (_pool == null || _config == null) return null;

            DamageNumberWidget widget;
            try { widget = _pool.Acquire(); }
            catch (System.InvalidOperationException) { return null; } // pool exhausted; non-fatal

            widget.Owner = _pool;

            Color color = kind switch
            {
                DamageNumberKind.Crit       => _config.dmgNumberColorCrit,
                DamageNumberKind.PlayerHurt => _config.dmgNumberColorPlayerHit,
                _                           => _config.dmgNumberColorNormal,
            };

            Vector3 jittered = worldPos + JitterOffset();
            widget.Configure(
                jittered,
                amount,
                color,
                _config.dmgNumberLifetime,
                _config.dmgNumberFloatHeight,
                Time.unscaledTime);

            return widget;
        }

        private Vector3 JitterOffset()
        {
            if (_config == null || _config.dmgNumberJitter <= 0f) return Vector3.zero;
            var v2 = _jitterTable[_jitterCursor];
            _jitterCursor = (_jitterCursor + 1) % _jitterTable.Length;
            return new Vector3(v2.x * _config.dmgNumberJitter, 0f, v2.y * _config.dmgNumberJitter);
        }
    }
}
