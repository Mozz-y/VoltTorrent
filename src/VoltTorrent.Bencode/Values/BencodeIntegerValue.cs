namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded integer value.
/// </summary>
/// <param name="Value">The decoded integer value.</param>
/// <param name="SourceRange">The byte range in the original input that produced this value.</param>
public readonly record struct BencodeIntegerValue(long Value, BencodeSourceRange SourceRange) : IBencodeValue;
