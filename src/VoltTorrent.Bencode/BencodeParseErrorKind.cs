namespace VoltTorrent.Bencode;

/// <summary>
/// Identifies the category of a Bencode parsing failure.
/// </summary>
public enum BencodeParseErrorKind
{
    None,
    EmptyInput,
    InvalidToken,
    MissingTerminator,
    EmptyInteger,
    InvalidInteger,
    LeadingZero,
    NegativeZero,
    Overflow
}