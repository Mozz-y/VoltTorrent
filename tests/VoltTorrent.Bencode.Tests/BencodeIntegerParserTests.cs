using FluentAssertions;
using Xunit;

namespace VoltTorrent.Bencode.Tests;

public sealed class BencodeIntegerParserTests
{
    [Theory]
    [InlineData("i0e", 0, 3)]
    [InlineData("i42e", 42, 4)]
    [InlineData("i-42e", -42, 5)]
    [InlineData("i9223372036854775807e", long.MaxValue, 21)]
    [InlineData("i-9223372036854775808e", long.MinValue, 22)]
    public void TryParse_WithValidInteger_ReturnsParsedValue(string input, long expectedValue, int expectedBytesConsumed)
    {
        var bytes = GetBytes(input);
        var result = BencodeIntegerParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Value.Should().Be(expectedValue);
        bytesConsumed.Should().Be(expectedBytesConsumed);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
        error.Offset.Should().Be(-1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("x42e")]
    [InlineData("i42")]
    [InlineData("ie")]
    [InlineData("i-e")]
    [InlineData("i04e")]
    [InlineData("i-0e")]
    [InlineData("i12xe")]
    [InlineData("i9223372036854775808e")]
    [InlineData("i-9223372036854775809e")]
    public void TryParse_WithInvalidInteger_ReturnsFalse(string input)
    {
        var bytes = GetBytes(input);
        var result = BencodeIntegerParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeFalse();
        value.Should().Be(default(BencodeInteger));
        bytesConsumed.Should().Be(0);
        error.Kind.Should().NotBe(BencodeParseErrorKind.None);
    }

    [Theory]
    [InlineData("", BencodeParseErrorKind.EmptyInput, 0)]
    [InlineData("x42e", BencodeParseErrorKind.InvalidToken, 0)]
    [InlineData("i42", BencodeParseErrorKind.MissingTerminator, 3)]
    [InlineData("ie", BencodeParseErrorKind.EmptyInteger, 1)]
    [InlineData("i-e", BencodeParseErrorKind.InvalidInteger, 1)]
    [InlineData("i04e", BencodeParseErrorKind.LeadingZero, 1)]
    [InlineData("i-0e", BencodeParseErrorKind.NegativeZero, 1)]
    [InlineData("i12xe", BencodeParseErrorKind.InvalidInteger, 3)]
    [InlineData("i9223372036854775808e", BencodeParseErrorKind.Overflow, 19)]
    [InlineData("i-9223372036854775809e", BencodeParseErrorKind.Overflow, 20)]
    public void TryParse_WithInvalidInteger_ReturnsExpectedError(string input, BencodeParseErrorKind expectedErrorKind, int expectedErrorOffset)
    {
        var bytes = GetBytes(input);
        var result = BencodeIntegerParser.TryParse(bytes, out _, out _, out var error);

        result.Should().BeFalse();
        error.Kind.Should().Be(expectedErrorKind);
        error.Offset.Should().Be(expectedErrorOffset);
        error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void TryParse_WithTrailingBytes_ConsumesOnlyInteger()
    {
        var bytes = GetBytes("i42e4:spam");
        var result = BencodeIntegerParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Value.Should().Be(42);
        bytesConsumed.Should().Be(4);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    private static byte[] GetBytes(string value)
    {
        return System.Text.Encoding.ASCII.GetBytes(value);
    }
}