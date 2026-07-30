namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded dictionary value.
/// </summary>
public sealed class BencodeDictionaryValue : IBencodeValue
{
    private readonly IReadOnlyList<BencodeDictionaryEntry> entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="BencodeDictionaryValue"/> class.
    /// </summary>
    /// <param name="entries">The parsed dictionary entries in source order.</param>
    /// <param name="sourceRange">The byte range in the original input that produced this value.</param>
    public BencodeDictionaryValue(IReadOnlyList<BencodeDictionaryEntry> entries, BencodeSourceRange sourceRange)
    {
        ArgumentNullException.ThrowIfNull(entries);
        this.entries = entries;
        SourceRange = sourceRange;
    }

    /// <summary>
    /// Gets the parsed dictionary entries in source order.
    /// </summary>
    public IReadOnlyList<BencodeDictionaryEntry> Entries => entries;

    /// <summary>
    /// Gets the number of entries contained by the dictionary.
    /// </summary>
    public int Count => entries.Count;

    /// <summary>
    /// Gets the byte range in the original input that produced this value.
    /// </summary>
    public BencodeSourceRange SourceRange { get; }
}