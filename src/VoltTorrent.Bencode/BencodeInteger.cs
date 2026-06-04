namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded integer value.
/// </summary>
/// <param name="Value">The decoded integer value.</param>
public readonly record struct BencodeInteger(long Value);