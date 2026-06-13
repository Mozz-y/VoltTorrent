namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded byte string value.
/// </summary>
/// <param name="Value">The decoded byte string payload.</param>
public readonly record struct BencodeByteString(ReadOnlyMemory<byte> Value)
{
    // Gets the number of bytes in the byte string payload.
    public int Length => Value.Length;
}
