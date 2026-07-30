namespace VoltTorrent.Bencode;

/// <summary>
/// Represents a single key-value pair inside a bencoded dictionary.
/// </summary>
public sealed class BencodeDictionaryEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BencodeDictionaryEntry"/> class.
    /// </summary>
    /// <param name="key">The byte string dictionary key.</param>
    /// <param name="value">The parsed value associated with the key.</param>
    public BencodeDictionaryEntry(BencodeByteStringValue key, IBencodeValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Key = key;
        Value = value;
    }

    /// <summary>
    /// Gets the byte string dictionary key.
    /// </summary>
    public BencodeByteStringValue Key { get; }

    /// <summary>
    /// Gets the parsed value associated with the key.
    /// </summary> 
    public IBencodeValue Value { get; }
}