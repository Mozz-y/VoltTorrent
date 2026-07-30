namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a parsed bencoded list value.
/// </summary>
public sealed class BencodeListValue : IBencodeValue
{
    private readonly IReadOnlyList<IBencodeValue> values;

    /// <summary>
    /// Initializes a new instance of the <see cref="BencodeListValue"/> class.
    /// </summary>
    /// <param name="values">The parsed values contained by the list.</param>
    /// <param name="sourceRange">The byte range in the original input that produced this value.</param>
    public BencodeListValue(IReadOnlyList<IBencodeValue> values, BencodeSourceRange sourceRange)
    {
        ArgumentNullException.ThrowIfNull(values);
        this.values = values.ToArray();
        SourceRange = sourceRange;
    }

    /// <summary>
    /// Gets the parsed values contained by the list.
    /// </summary>
    public IReadOnlyList<IBencodeValue> Values => values;

    /// <summary>
    /// Gets the number of values contained by the list.
    /// </summary>
    public int Count => values.Count;

    /// <summary>
    /// Gets the byte range in the original input that produced this value.
    /// </summary>
    public BencodeSourceRange SourceRange { get; }
}