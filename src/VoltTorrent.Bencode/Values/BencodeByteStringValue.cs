namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded byte string value.
/// </summary>
/// <param name="Value">The decoded byte string payload.</param>
public readonly record struct BencodeByteStringValue(ReadOnlyMemory<byte> Value)
{
    /// <summary>
    /// Gets the number of bytes in the byte string payload.
    /// </summary>
    public int Length => Value.Length;
}
