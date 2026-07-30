namespace VoltTorrent.Bencode;

/// <summary>
/// Represents any parsed Bencode value.
/// </summary>
public interface IBencodeValue
{
    // Gets the byte range in the original input that produced this value.
    BencodeSourceRange SourceRange { get; }
}