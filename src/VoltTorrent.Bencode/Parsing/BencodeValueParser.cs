namespace VoltTorrent.Bencode;

public static class BencodeValueParser
{
    private const int MaxDepth = 128;

    /// <summary>
    /// Attempts to parse a bencoded value from the beginning of the provided input.
    /// </summary>
    /// <param name="input">The byte input that should begin with a bencoded value.</param>
    /// <param name="value">The parsed bencoded value when parsing succeeds.</param>
    /// <param name="bytesConsumed">The number of bytes consumed when parsing succeeds.</param>
    /// <param name="error">The parse error when parsing fails.</param>
    /// <returns><c>true</c> when a bencoded value was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlyMemory<byte> input, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        return TryParseValue(input, 0, 0, out value, out bytesConsumed, out error);
    }

    /// <summary>
    /// Attempts to parse a bencoded value at the provided offset.
    /// </summary>
    /// <param name="input">The original byte input being parsed.</param>
    /// <param name="startOffset">The offset where the value starts.</param>
    /// <param name="depth">The current recursive parsing depth.</param>
    /// <param name="value">The parsed value when parsing succeeds.</param>
    /// <param name="bytesConsumed">The number of bytes consumed from <paramref name="startOffset"/>.</param>
    /// <param name="error">The parse error when parsing fails.</param>
    /// <returns><c>true</c> when a value was parsed successfully; otherwise, <c>false</c>.</returns>
    private static bool TryParseValue(ReadOnlyMemory<byte> input, int startOffset, int depth, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        value = null;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        // Stop malicious or broken input from creating extremely deep recursion.
        if (depth > MaxDepth)
        {
            error = new BencodeParseError(BencodeParseErrorKind.MaxDepthExceeded, startOffset, "Bencode nesting is too deep.");
            return false;
        }

        // There must be at least one byte available to decide which value type this is.
        if (startOffset >= input.Length)
        {
            error = new BencodeParseError(BencodeParseErrorKind.EmptyInput, startOffset, "Input is empty.");
            return false;
        }

        var firstByte = input.Span[startOffset];

        // Integers always start with 'i', for example i42e.
        if (firstByte == (byte)'i')
        {
            return TryParseInteger(input, startOffset, out value, out bytesConsumed, out error);
        }

        // Byte strings always start with a digit, for example 4:spam.
        if (IsByteStringStart(firstByte))
        {
            return TryParseByteString(input, startOffset, out value, out bytesConsumed, out error);
        }

        // Lists always start with 'l', for example li1ei2ee.
        if (firstByte == (byte)'l')
        {
            return TryParseList(input, startOffset, depth, out value, out bytesConsumed, out error);
        }

        // Dictionaries always start with 'd', for example d3:fooi42ee.
        if (firstByte == (byte)'d')
        {
            return TryParseDictionary(input, startOffset, depth, out value, out bytesConsumed, out error);
        }

        error = new BencodeParseError(BencodeParseErrorKind.InvalidToken, startOffset, "Input does not begin with supported bencoded value.");
        return false;
    }

    /// <summary>
    /// Attempts to parse a bencoded integer at the provided offset.
    /// </summary>
    private static bool TryParseInteger(ReadOnlyMemory<byte> input, int startOffset, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        value = null;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        // The integer parser expects its input to start directly at the integer token.
        if (!BencodeIntegerParser.TryParse(input.Span[startOffset..], out var integerValue, out bytesConsumed, out error))
        {
            error = AddOffset(error, startOffset);
            return false;
        }

        value = new BencodeIntegerValue(integerValue.Value, AddOffset(integerValue.SourceRange, startOffset));
        return true;
    }

    /// <summary>
    /// Attempts to parse a bencoded byte string at the provided offset.
    /// </summary>
    private static bool TryParseByteString(ReadOnlyMemory<byte> input, int startOffset, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        value = null;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        // The byte string parser expects a slice that begins at the length prefix.
        if (!BencodeByteStringParser.TryParse(input.Slice(startOffset), out var byteStringValue, out bytesConsumed, out error))
        {
            error = AddOffset(error, startOffset);
            return false;
        }

        value = new BencodeByteStringValue(byteStringValue.Value, AddOffset(byteStringValue.SourceRange, startOffset));
        return true;
    }

    /// <summary>
    /// Attempts to parse a bencoded list at the provided offset.
    /// </summary>
    private static bool TryParseList(ReadOnlyMemory<byte> input, int startOffset, int depth, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        value = null;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        var values = new List<IBencodeValue>();
        var position = startOffset + 1;

        while (true)
        {
             // A list must eventually end with 'e'; reaching the end first means the input is malformed.
            if (position >= input.Length)
            {
                error = new BencodeParseError(BencodeParseErrorKind.UnterminatedList, input.Length, "Bencoded list is missing its 'e' terminator.");
                return false;
            }

            // The 'e' byte closes the list and means no more values belong to this list.
            if (input.Span[position] == (byte)'e')
            {
                var parsedBytesConsumed = position - startOffset + 1;
                value = new BencodeListValue(values, new BencodeSourceRange(startOffset, parsedBytesConsumed));
                bytesConsumed = parsedBytesConsumed;
                return true;
            }

            // List items can be any bencode value, so parsing delegates back to the general parser.
            if (!TryParseValue(input, position, depth + 1, out var nestedValue, out var nestedBytesConsumed, out error))
            {
                return false;
            }

            values.Add(nestedValue!);
            position += nestedBytesConsumed;
        }
    }

    /// <summary>
    /// Attempts to parse a bencoded dictionary at the provided offset.
    /// </summary>
    private static bool TryParseDictionary(ReadOnlyMemory<byte> input, int startOffset, int depth, out IBencodeValue? value, out int bytesConsumed, out BencodeParseError error)
    {
        value = null;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        var entries = new List<BencodeDictionaryEntry>();
        var position = startOffset + 1;
        ReadOnlyMemory<byte>? previousKey = null;

        while (true)
        {
            // A dictionary must eventually end with 'e'; reaching the end first means it is unterminated.
            if (position >= input.Length)
            {
                error = new BencodeParseError(BencodeParseErrorKind.UnterminatedDictionary, input.Length, "Bencoded dictionary is missing its 'e' terminator.");
                return false;
            }

             // The 'e' byte closes the dictionary and means there are no more key-value pairs.
            if (input.Span[position] == (byte)'e')
            {
                var parsedBytesConsumed = position - startOffset + 1;
                value = new BencodeDictionaryValue(entries, new BencodeSourceRange(startOffset, parsedBytesConsumed));
                bytesConsumed = parsedBytesConsumed;
                return true;
            }

            // Dictionary keys are required by the bencode format to be byte strings.
            if (!IsByteStringStart(input.Span[position]))
            {
                error = new BencodeParseError(BencodeParseErrorKind.InvalidDictionaryKey, position, "Bencode dictionary keys must be byte strings.");
                return false;
            }

            // Parse the key first; after this, position can advance to the value.
            if (!BencodeByteStringParser.TryParse(input.Slice(position), out var key, out var keyBytesConsumed, out error))
            {
                error = AddOffset(error, position);
                return false;
            }

            // Dictionary keys must appear in ascending raw byte order for canonical bencode.
            if (previousKey.HasValue && CompareByteStrings(previousKey.Value.Span, key.Value.Span) >= 0)
            {
                error = new BencodeParseError(BencodeParseErrorKind.UnsortedDictionaryKeys, position, "Bencode dictionary keys must be sorted in ascending raw byte order.");
                return false;
            }

            position += keyBytesConsumed;

            // A key without a following value is not a complete dictionary entry.
            if (position >= input.Length || input.Span[position] == (byte)'e')
            {
                error = new BencodeParseError(BencodeParseErrorKind.UnterminatedDictionary, position, "Bencoded dictionary key is missing its value.");
                return false;
            }

            // Dictionary values can be any bencode value, including nested lists or dictionaries.
            if (!TryParseValue(input, position, depth + 1, out var entryValue, out var valueBytesConsumed, out error))
            {
                return false;
            }

            entries.Add(new BencodeDictionaryEntry(key, entryValue!));
            previousKey = key.Value;
            position += valueBytesConsumed;
        }
    }

    /// <summary>
    /// Determines whether the provided byte can start a bencoded byte string.
    /// </summary>
    /// <param name="value">The byte to inspect.</param>
    /// <returns><c>true</c> when the byte is an ASCII digit; otherwise, <c>false</c>.</returns>
    private static bool IsByteStringStart(byte value)
    {
        return value >= (byte)'0' && value <= (byte)'9';
    }

    /// <summary>
    /// Compares two byte strings using raw lexicographic byte order.
    /// </summary>
    /// <param name="left">The first byte string.</param>
    /// <param name="right">The second byte string.</param>
    /// <returns>A negative value when left comes first, zero when equal, or a positive value when right comes first.</returns>
    private static int CompareByteStrings(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        var minimumLength = Math.Min(left.Length, right.Length);

        for (var index = 0; index < minimumLength; index++)
        {
            if (left[index] != right[index])
            {
                return left[index].CompareTo(right[index]);
            }
        }

        return left.Length.CompareTo(right.Length);
    }

    /// <summary>
    /// Converts an error from a sliced parser call back to an offset in the original input.
    /// </summary>
    /// <param name="error">The parse error returned by the sliced parser.</param>
    /// <param name="offset">The offset where the sliced parser started.</param>
    /// <returns>The same parse error with its offset adjusted to the original input.</returns>
    private static BencodeParseError AddOffset(BencodeParseError error, int offset)
    {
        if (error.Kind == BencodeParseErrorKind.None)
        {
            return error;
        }

        return new BencodeParseError(error.Kind, error.Offset + offset, error.Message);
    }

    /// <summary>
    /// Converts a source range from a sliced parser call back to a range in the original input.
    /// </summary>
    /// <param name="sourceRange">The source range returned by the sliced parser.</param>
    /// <param name="offset">The offset where the sliced parser started.</param>
    /// <returns>The same source range adjusted to the original input.</returns>
    private static BencodeSourceRange AddOffset(BencodeSourceRange sourceRange, int offset)
    {
        return new BencodeSourceRange(sourceRange.Offset + offset, sourceRange.Length);
    }
}