namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded byte string value.
/// </summary>
/// <param name="Value">The decoded byte string payload.</param>
/// <param name="SourceRange">The byte range in the original input that produced this value.</param>
public readonly record struct BencodeByteStringValue(ReadOnlyMemory<byte> Value, BencodeSourceRange SourceRange) : IBencodeValue
{
    /// <summary>
    /// Gets the number of bytes in the byte string payload.
    /// </summary>
    public int Length => Value.Length;
}
