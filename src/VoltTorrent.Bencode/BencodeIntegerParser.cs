namespace VoltTorrent.Bencode;

/// <summary>
/// Parses bencoded integer values from byte-oriented input.
/// </summary>
public static class BencodeIntegerParser
{
    /// <summary>
    /// Attempts to parse a bencoded integer from the beginning of the provided input.
    /// </summary>
    /// <param name="input">The byte input that should begin with a bencoded integer.</param>
    /// <param name="value">The parsed integer value when parsing succeeds.</param>
    /// <param name="bytesConsumed">The number of bytes consumed when parsing succeeds.</param>
    /// <param name="error">The parse error when parsing fails.</param>
    /// <returns><c>true</c> when an integer was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlySpan<byte> input, out BencodeInteger value, out int bytesConsumed, out BencodeParseError error)
    {
        value = default;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        if (input.IsEmpty)
        {
            error = new BencodeParseError(BencodeParseErrorKind.EmptyInput, 0, "Input is empty.");
            return false;
        }

        if (input[0] != (byte)'i')
        {
            error = new BencodeParseError(BencodeParseErrorKind.InvalidToken, 0, "Expected 'i' at the beginning of an integer.");
            return false;
        }

        var terminatorIndex = input[1..].IndexOf((byte)'e');

        if (terminatorIndex < 0)
        {
            error = new BencodeParseError(BencodeParseErrorKind.MissingTerminator, input.Length, "Missing 'e' terminator for integer.");
            return false;
        }

        var integerBytes = input.Slice(1, terminatorIndex);
        var parsedBytesConsumed = terminatorIndex + 2; // 'i' + integer bytes + 'e'

        if (integerBytes.IsEmpty)
        {
            error = new BencodeParseError(BencodeParseErrorKind.EmptyInteger, 1, "Integer value is empty.");
            return false;
        }

        var isNegative = integerBytes[0] == (byte)'-';
        var digitStart = isNegative ? 1 : 0;

        if (isNegative && integerBytes.Length == 1)
        {
            error = new BencodeParseError(BencodeParseErrorKind.InvalidInteger, 1, "Negative sign without digits.");
            return false;
        }

        if (integerBytes[digitStart] == (byte)'0' && integerBytes.Length - digitStart > 1)
        {
            error = new BencodeParseError(BencodeParseErrorKind.LeadingZero, 1 + digitStart, "Bencoded integers cannot contain leading zeroes.");
            return false;
        }

        if (isNegative && integerBytes[digitStart] == (byte)'0')
        {
            error = new BencodeParseError(BencodeParseErrorKind.NegativeZero, 1, "Negative zero is not allowed in bencoded integers.");
            return false;
        }

        long parsedValue = 0;

        for (var index = digitStart; index < integerBytes.Length; index++)
        {
            var currentByte = integerBytes[index];
            if (currentByte < (byte)'0' || currentByte > (byte)'9')
            {
                error = new BencodeParseError(BencodeParseErrorKind.InvalidInteger, 1 + index, "Bencoded integer contains a non-digit character.");
                return false;
            }

            var digit = currentByte - (byte)'0';

            // Check for overflow before multiplying and adding.
            if (isNegative)
            {
                if (parsedValue < (long.MinValue + digit) / 10)
                {
                    error = new BencodeParseError(BencodeParseErrorKind.Overflow, 1 + index, "Bencoded integer is smaller than Int64.MinValue.");
                    return false;
                }
                parsedValue = (parsedValue * 10) - digit;
            }
            else
            {
                if (parsedValue > (long.MaxValue - digit) / 10)
                {
                    error = new BencodeParseError(BencodeParseErrorKind.Overflow, 1 + index, "Bencoded integer is larger than Int64.MaxValue.");
                    return false;
                }
                parsedValue = (parsedValue * 10) + digit;
            }
        }

        value = new BencodeInteger(parsedValue);
        bytesConsumed = parsedBytesConsumed;
        return true;
    }
}