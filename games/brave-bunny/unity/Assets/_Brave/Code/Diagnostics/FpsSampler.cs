// FpsSampler — rolling 1-second average FPS tracker.
// Attach to any persistent GameObject (e.g. MainCamera) in a stress scene.
// Reads via FpsSampler.AverageFps from tests or runtime overlays.
// No allocations on the hot path: uses a fixed-size ring buffer.

using UnityEngine;

namespace Brave.Diagnostics
{
    /// <summary>
    /// Tracks a 1-second rolling-window average of unscaled FPS.
    /// Uses a fixed-size ring buffer — zero per-frame GC allocations.
    /// </summary>
    public sealed class FpsSampler : MonoBehaviour
    {
        // ---- constants ----
        private const int BufferSize = 128;  // enough for 60 fps over > 2 s window

        // ---- state ----
        private readonly float[] _dtBuffer = new float[BufferSize];
        private int   _head;         // next-write index
        private float _accumulator;  // sum of all values currently in the ring
        private int   _filled;       // how many slots are populated (ramps up to BufferSize)
        private float _windowTime;   // sum of dt within the 1-second window

        /// <summary>Last-computed 1-second rolling-window average FPS.</summary>
        public float AverageFps { get; private set; }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f) return;

            // Evict the oldest sample from the accumulator.
            float evicted = _dtBuffer[_head];
            _accumulator -= evicted;
            _windowTime  -= evicted;

            // Write the new sample.
            _dtBuffer[_head] = dt;
            _accumulator += dt;
            _windowTime  += dt;
            _head = (_head + 1) % BufferSize;
            if (_filled < BufferSize) _filled++;

            // Trim the window to the last 1.0 second.
            // Walk the ring backward, dropping frames older than 1 s.
            float window = 1.0f;
            int count = 0;
            float sum = 0f;
            for (int i = 0; i < _filled; i++)
            {
                int idx = (_head - 1 - i + BufferSize) % BufferSize;
                sum += _dtBuffer[idx];
                if (sum > window) break;
                count++;
            }

            AverageFps = count > 0 ? count / Mathf.Min(sum, window) : 1f / dt;
        }
    }
}
