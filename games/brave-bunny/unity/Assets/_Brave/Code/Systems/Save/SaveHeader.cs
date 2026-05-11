// Brave Bunny — Systems / Save
// Tech spec: docs/06-tech-spec/03-save-system.md (file format table)
// ADR-0008: Newtonsoft JSON inside a binary wrapper.

#nullable enable

using System;

namespace Brave.Systems.Save;

/// <summary>
/// 14-byte fixed-length header that precedes every save payload on disk.
/// Layout matches 03-save-system.md exactly:
/// <code>
///   0  : 4  magic "BRBN"
///   4  : 2  version (uint16 LE)
///   6  : 4  payload length (uint32 LE)
///   10 : 4  payload CRC32 (uint32 LE)
/// </code>
/// </summary>
public readonly struct SaveHeader
{
    public const int Size = 14;
    public const int CurrentVersion = 1;

    /// <summary>ASCII "BRBN" → 0x4252_424E little-endian on disk.</summary>
    public static readonly byte[] Magic = { (byte)'B', (byte)'R', (byte)'B', (byte)'N' };

    public readonly ushort Version;
    public readonly uint PayloadLength;
    public readonly uint PayloadCrc32;

    public SaveHeader(ushort version, uint payloadLength, uint payloadCrc32)
    {
        Version = version;
        PayloadLength = payloadLength;
        PayloadCrc32 = payloadCrc32;
    }

    /// <summary>Serialize the header into a 14-byte buffer using little-endian uints.</summary>
    public byte[] ToBytes()
    {
        var buf = new byte[Size];
        Buffer.BlockCopy(Magic, 0, buf, 0, 4);
        buf[4] = (byte)(Version & 0xFF);
        buf[5] = (byte)((Version >> 8) & 0xFF);
        WriteUInt32LE(buf, 6, PayloadLength);
        WriteUInt32LE(buf, 10, PayloadCrc32);
        return buf;
    }

    /// <summary>Parse a 14-byte header. Throws on bad magic.</summary>
    public static SaveHeader FromBytes(byte[] buf)
    {
        if (buf == null || buf.Length < Size)
            throw new InvalidSaveException("Header too short.");
        if (buf[0] != Magic[0] || buf[1] != Magic[1] || buf[2] != Magic[2] || buf[3] != Magic[3])
            throw new InvalidSaveException("Bad magic (expected BRBN).");

        var version = (ushort)(buf[4] | (buf[5] << 8));
        var length = ReadUInt32LE(buf, 6);
        var crc = ReadUInt32LE(buf, 10);
        return new SaveHeader(version, length, crc);
    }

    private static void WriteUInt32LE(byte[] buf, int offset, uint value)
    {
        buf[offset + 0] = (byte)(value & 0xFF);
        buf[offset + 1] = (byte)((value >> 8) & 0xFF);
        buf[offset + 2] = (byte)((value >> 16) & 0xFF);
        buf[offset + 3] = (byte)((value >> 24) & 0xFF);
    }

    private static uint ReadUInt32LE(byte[] buf, int offset) =>
        (uint)(buf[offset] | (buf[offset + 1] << 8) | (buf[offset + 2] << 16) | (buf[offset + 3] << 24));
}

/// <summary>Thrown by header parsing + payload validation on a corrupt save candidate.</summary>
public sealed class InvalidSaveException : Exception
{
    public InvalidSaveException(string message) : base(message) { }
}
