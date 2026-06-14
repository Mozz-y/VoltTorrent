namespace VoltTorrent.Bencode;

/// <summary>
/// Parses bencoded byte string values from byte-oriented input.
/// </summary>
public static class BencodeByteStringParser
{
    /// <summary>
    /// Attempts to parse a bencoded byte string from the beginning of the provided input.
    /// </summary>
    /// <param name="input">The byte input that should begin with a bencoded byte string.</param>
    /// <param name="value">The parsed byte string value when parsing succeeds.</param>
    /// <param name="bytesConsumed">The number of bytes consumed when parsing succeeds.</param>
    /// <param name="error">The parse error when parsing fails.</param>
    /// <returns><c>true</c> when a byte string was parsed successfully; otherwise, <c>false</c>.</returns>
    public static bool TryParse(ReadOnlyMemory<byte> input, out BencodeByteStringValue value, out int bytesConsumed, out BencodeParseError error)
    {
        value = default;
        bytesConsumed = 0;
        error = BencodeParseError.None;

        if (input.IsEmpty)
        {
            error = new BencodeParseError(BencodeParseErrorKind.EmptyInput, 0, "Input is empty.");
            return false;
        }

        var inputSpan = input.Span;
        var separatorIndex = inputSpan.IndexOf((byte)':');

        if (separatorIndex < 0)
        {
            error = new BencodeParseError(BencodeParseErrorKind.MissingSeparator, input.Length, "Missing ':' separator for byte string.");
            return false;
        }

        var lengthBytes = inputSpan[..separatorIndex];

        if (lengthBytes.IsEmpty)
        {
            error = new BencodeParseError(BencodeParseErrorKind.EmptyByteStringLength, 0, "Byte string length is empty.");
            return false;
        }

        if (lengthBytes[0] == (byte)'0' && lengthBytes.Length > 1)
        {
            error = new BencodeParseError(BencodeParseErrorKind.LeadingZero, 0, "Byte string length cannot contain leading zeroes.");
            return false;
        }

        var byteStringLength = 0;

        for (var index = 0; index < lengthBytes.Length; index++)
        {
            var currentByte = lengthBytes[index];

            if (currentByte < (byte)'0' || currentByte > (byte)'9')
            {
                error = new BencodeParseError(BencodeParseErrorKind.InvalidByteStringLength, index, "Byte string length contains a non-digit character.");
                return false;
            }

            var digit = currentByte - (byte)'0';

            // Check before multiplying so invalid large lengths fail without overflowing.
            if (byteStringLength > (int.MaxValue - digit) / 10)
            {
                error = new BencodeParseError(BencodeParseErrorKind.Overflow, index, "Byte string length is larger than Int32.MaxValue.");
                return false;
            }

            byteStringLength = (byteStringLength * 10) + digit;
        }

        var payloadStart = separatorIndex + 1;

        // Compare against remaining input before adding to avoid integer overflow.
        if (byteStringLength > input.Length - payloadStart)
        {
            error = new BencodeParseError(BencodeParseErrorKind.TruncatedByteString, payloadStart, "Byte string payload is shorter than the declared length.");
            return false;
        }

        var totalBytesConsumed = payloadStart + byteStringLength;

        value = new BencodeByteStringValue(input.Slice(payloadStart, byteStringLength));
        bytesConsumed = totalBytesConsumed;

        return true;
    }
}
