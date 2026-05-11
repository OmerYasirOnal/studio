// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (CRC32 of payload in header)
// Standard IEEE 802.3 polynomial 0xEDB88320, reflected. Vanilla table-based
// implementation; not a hot path (runs once per save, payload ≤ 50 KB).

#nullable enable

namespace Brave.Systems.Save;

internal static class Crc32
{
    private static readonly uint[] _table = BuildTable();

    private static uint[] BuildTable()
    {
        const uint poly = 0xEDB88320u;
        var t = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var c = i;
            for (var j = 0; j < 8; j++) c = (c & 1) != 0 ? (poly ^ (c >> 1)) : (c >> 1);
            t[i] = c;
        }
        return t;
    }

    public static uint Compute(byte[] buffer)
    {
        var crc = 0xFFFFFFFFu;
        for (var i = 0; i < buffer.Length; i++) crc = (crc >> 8) ^ _table[(crc ^ buffer[i]) & 0xFF];
        return crc ^ 0xFFFFFFFFu;
    }
}
