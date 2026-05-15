// Brave Bunny — Systems / Diagnostics / CrashLogBuffer
// Wave 11 — rolling fixed-size ring buffer of the last N Application.logMessageReceived
// entries. Captured to provide context lines in a CrashReport.contextLog payload.
//
// Pure C#, allocation-free per push (sole allocation is the wrapping LogLine value type
// — a struct with three immutable strings/enum). EditMode-tested in isolation; no
// UnityEngine.Application dependency aside from the LogType enum which lives in
// UnityEngine.CoreModule and is always available.
//
// NO personally-identifiable info is captured here — the buffer mirrors whatever the
// game emits via Debug.Log/Warning/Error. By policy, no save-data, no user-typed strings,
// no analytics IDs are routed through Debug.* — see docs/06-tech-spec/03-save-system.md.

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brave.Systems.Diagnostics
{
    /// <summary>
    /// One captured log line: condensed message + stack snippet + Unity LogType.
    /// Immutable; safe to enumerate across threads (the buffer itself is single-writer).
    /// </summary>
    public readonly struct CrashLogLine
    {
        public readonly string Message;
        public readonly string StackTrace;
        public readonly LogType Type;
        public readonly DateTime UtcTimestamp;

        public CrashLogLine(string message, string stackTrace, LogType type, DateTime utcTimestamp)
        {
            Message = message ?? string.Empty;
            StackTrace = stackTrace ?? string.Empty;
            Type = type;
            UtcTimestamp = utcTimestamp;
        }
    }

    /// <summary>
    /// Fixed-capacity ring buffer of <see cref="CrashLogLine"/> entries.
    /// New entries overwrite the oldest once capacity is hit (FIFO eviction).
    /// </summary>
    public sealed class CrashLogBuffer
    {
        public const int DefaultCapacity = 50;

        private readonly CrashLogLine[] _ring;
        private readonly int _capacity;
        private int _head;     // next write index
        private int _filled;   // number of valid entries (0..capacity)

        public int Capacity => _capacity;
        public int Count => _filled;

        public CrashLogBuffer(int capacity = DefaultCapacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be > 0");
            _capacity = capacity;
            _ring = new CrashLogLine[capacity];
        }

        /// <summary>Append a line. Wraps around when the buffer is full (FIFO eviction).</summary>
        public void Push(CrashLogLine line)
        {
            _ring[_head] = line;
            _head = (_head + 1) % _capacity;
            if (_filled < _capacity) _filled++;
        }

        /// <summary>
        /// Snapshot the buffer in chronological order (oldest first → newest last).
        /// Allocates a new list for the caller — only invoked on crash (cold path).
        /// </summary>
        public IReadOnlyList<CrashLogLine> Snapshot()
        {
            var list = new List<CrashLogLine>(_filled);
            if (_filled == 0) return list;

            // When filled < capacity, entries live in [0.._filled). When full,
            // the oldest entry is at _head (the next write slot) and wraps around.
            int start = _filled < _capacity ? 0 : _head;
            for (int i = 0; i < _filled; i++)
            {
                list.Add(_ring[(start + i) % _capacity]);
            }
            return list;
        }

        /// <summary>Clear all buffered lines (used by tests; no production caller).</summary>
        public void Clear()
        {
            Array.Clear(_ring, 0, _capacity);
            _head = 0;
            _filled = 0;
        }
    }
}
