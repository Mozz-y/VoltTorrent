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
    MissingSeparator,
    EmptyByteStringLength,
    EmptyInteger,
    InvalidByteStringLength,
    InvalidInteger,
    LeadingZero,
    NegativeZero,
    Overflow,
    TruncatedByteString
}