namespace VoltTorrent.Bencode;

/// <summary>
/// Describes a Bencode parsing failure.
/// </summary>
/// <param name="Kind">The machine-readable error category.</param>
/// <param name="Offset">The zero-based byte offset where the error was detected.</param>
/// <param name="Message">A human-readable explanation of the error.</param>
public readonly record struct BencodeParseError(BencodeParseErrorKind Kind, int Offset, string Message)
{
    // Gets an empty parse error value used when parsing succeeds.
    public static BencodeParseError None { get; } = new BencodeParseError(BencodeParseErrorKind.None, -1, string.Empty);
}