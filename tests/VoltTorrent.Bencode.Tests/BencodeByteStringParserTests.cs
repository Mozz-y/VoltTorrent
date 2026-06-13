using FluentAssertions;
using System.Text;
using Xunit;

namespace VoltTorrent.Bencode.Tests;

public sealed class BencodeByteStringParserTests
{
    [Theory]
    [InlineData("0:", "", 2)]
    [InlineData("4:spam", "spam", 6)]
    [InlineData("5:hello", "hello", 7)]
    public void TryParse_WithValidByteString_ReturnsParsedValue(string input, string expectedValue, int expectedBytesConsumed)
    {
        var bytes = GetBytes(input);
        var result = BencodeByteStringParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        GetString(value.Value).Should().Be(expectedValue);
        value.Length.Should().Be(expectedValue.Length);
        bytesConsumed.Should().Be(expectedBytesConsumed);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
        error.Offset.Should().Be(-1);
    }

    [Fact]
    public void TryParse_WithTrailingBytes_ConsumesOnlyByteString()
    {
        var bytes = GetBytes("4:spam5:hello");
        var result = BencodeByteStringParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        GetString(value.Value).Should().Be("spam");
        bytesConsumed.Should().Be(6);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    [Fact]
    public void TryParse_WithBinaryPayload_PreservesRawBytes()
    {
        var bytes = new byte[] { (byte)'3', (byte)':', 0x00, 0xFF, 0x41 };
        var result = BencodeByteStringParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Value.ToArray().Should().Equal(0x00, 0xFF, 0x41);
        value.Length.Should().Be(3);
        bytesConsumed.Should().Be(5);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    [Theory]
    [InlineData("")]
    [InlineData(":spam")]
    [InlineData("x:spam")]
    [InlineData("4spam")]
    [InlineData("4-spam")]
    [InlineData("04:spam")]
    [InlineData("5:spam")]
    [InlineData("2147483648:")]
    [InlineData("2147483647:")]
    public void TryParse_WithInvalidByteString_ReturnsFalse(string input)
    {
        var bytes = GetBytes(input);
        var result = BencodeByteStringParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeFalse();
        value.Length.Should().Be(0);
        bytesConsumed.Should().Be(0);
        error.Kind.Should().NotBe(BencodeParseErrorKind.None);
        error.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("", BencodeParseErrorKind.EmptyInput, 0)]
    [InlineData(":spam", BencodeParseErrorKind.EmptyByteStringLength, 0)]
    [InlineData("x:spam", BencodeParseErrorKind.InvalidByteStringLength, 0)]
    [InlineData("4spam", BencodeParseErrorKind.MissingSeparator, 5)]
    [InlineData("4-spam", BencodeParseErrorKind.MissingSeparator, 6)]
    [InlineData("04:spam", BencodeParseErrorKind.LeadingZero, 0)]
    [InlineData("5:spam", BencodeParseErrorKind.TruncatedByteString, 2)]
    [InlineData("2147483648:", BencodeParseErrorKind.Overflow, 9)]
    [InlineData("2147483647:", BencodeParseErrorKind.TruncatedByteString, 11)]
    public void TryParse_WithInvalidByteString_ReturnsExpectedError(string input, BencodeParseErrorKind expectedKind, int expectedOffset)
    {
        var bytes = GetBytes(input);
        var result = BencodeByteStringParser.TryParse(bytes, out _, out _, out var error);

        result.Should().BeFalse();
        error.Kind.Should().Be(expectedKind);
        error.Offset.Should().Be(expectedOffset);
        error.Message.Should().NotBeNullOrWhiteSpace();
    }

    private static byte[] GetBytes(string value)
    {
        return Encoding.ASCII.GetBytes(value);
    }

    private static string GetString(ReadOnlyMemory<byte> value)
    {
        return Encoding.ASCII.GetString(value.Span);
    }
}
