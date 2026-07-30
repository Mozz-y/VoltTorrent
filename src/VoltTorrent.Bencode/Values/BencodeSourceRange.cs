namespace VoltTorrent.Bencode;

/// <summary>
/// Represents the byte range in the original bencoded input that produced a parsed value.
/// </summary>
public readonly record struct BencodeSourceRange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BencodeSourceRange"/> struct.
    /// </summary>
    /// <param name="offset">The zero-based offset where the value starts.</param>
    /// <param name="length">The number of bytes occupied by the value.</param>
    public BencodeSourceRange(int offset, int length)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Source offset cannot be negative.");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Source length cannot be negative.");
        }

        if (length > int.MaxValue - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Source length is too large for the given offset.");
        }

        Offset = offset;
        Length = length;
    }

    public int Offset { get; }
    public int Length { get; }
    public int EndOffset => Offset + Length;

    /// <summary>
    /// Slices the provided source input using this range.
    /// </summary>
    /// <param name="source">The original source input.</param>
    /// <returns>The bytes covered by this source range.</returns>
    public ReadOnlyMemory<byte> Slice(ReadOnlyMemory<byte> source)
    {
        if (EndOffset > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(source), "Source range extends past the end of the provided source.");
        }

        return source.Slice(Offset, Length);
    }
}