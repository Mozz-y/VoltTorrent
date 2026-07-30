using FluentAssertions;
using System.Text;
using Xunit;

namespace VoltTorrent.Bencode.Tests;

public sealed class BencodeValueParserTests
{
    [Fact]
    public void TryParse_WithInteger_ReturnsIntegerValue()
    {
        var bytes = GetBytes("i42e");
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Should().BeOfType<BencodeIntegerValue>();

        var integerValue = (BencodeIntegerValue)value!;

        integerValue.Value.Should().Be(42);
        integerValue.SourceRange.Should().Be(new BencodeSourceRange(0, 4));
        bytesConsumed.Should().Be(4);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    [Fact]
    public void TryParse_WithByteString_ReturnsByteStringValue()
    {
        var bytes = GetBytes("4:spam");
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Should().BeOfType<BencodeByteStringValue>();

        var byteStringValue = (BencodeByteStringValue)value!;

        GetString(byteStringValue.Value).Should().Be("spam");
        byteStringValue.SourceRange.Should().Be(new BencodeSourceRange(0, 6));
        bytesConsumed.Should().Be(6);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    [Fact]
    public void TryParse_WithNestedList_ReturnsListValue()
    {
        var bytes = GetBytes("li42e4:spaml3:fooed1:ai1eee");
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Should().BeOfType<BencodeListValue>();

        var listValue = (BencodeListValue)value!;

        listValue.Count.Should().Be(4);
        listValue.SourceRange.Should().Be(new BencodeSourceRange(0, bytes.Length));
        listValue.SourceRange.Slice(bytes).ToArray().Should().Equal(bytes);
        bytesConsumed.Should().Be(bytes.Length);
        error.Kind.Should().Be(BencodeParseErrorKind.None);

        listValue.Values[0].Should().BeOfType<BencodeIntegerValue>();
        listValue.Values[1].Should().BeOfType<BencodeByteStringValue>();
        listValue.Values[2].Should().BeOfType<BencodeListValue>();
        listValue.Values[3].Should().BeOfType<BencodeDictionaryValue>();
    }

    [Fact]
    public void TryParse_WithDictionary_ReturnsDictionaryEntries()
    {
        var bytes = GetBytes("d1:ai1e1:b4:spame");
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Should().BeOfType<BencodeDictionaryValue>();

        var dictionaryValue = (BencodeDictionaryValue)value!;

        dictionaryValue.Count.Should().Be(2);
        dictionaryValue.SourceRange.Should().Be(new BencodeSourceRange(0, bytes.Length));
        bytesConsumed.Should().Be(bytes.Length);
        error.Kind.Should().Be(BencodeParseErrorKind.None);

        GetString(dictionaryValue.Entries[0].Key.Value).Should().Be("a");
        dictionaryValue.Entries[0].Value.Should().BeOfType<BencodeIntegerValue>();

        GetString(dictionaryValue.Entries[1].Key.Value).Should().Be("b");
        dictionaryValue.Entries[1].Value.Should().BeOfType<BencodeByteStringValue>();
    }

    [Fact]
    public void TryParse_WithTrailingBytes_ConsumesOnlyFirstValue()
    {
        var bytes = GetBytes("li1ee4:spam");
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeTrue();
        value.Should().BeOfType<BencodeListValue>();
        bytesConsumed.Should().Be(5);
        error.Kind.Should().Be(BencodeParseErrorKind.None);
    }

    [Theory]
    [InlineData("", BencodeParseErrorKind.EmptyInput, 0)]
    [InlineData("x", BencodeParseErrorKind.InvalidToken, 0)]
    [InlineData("li1e", BencodeParseErrorKind.UnterminatedList, 4)]
    [InlineData("d1:ai1e", BencodeParseErrorKind.UnterminatedDictionary, 7)]
    [InlineData("d1:ae", BencodeParseErrorKind.UnterminatedDictionary, 4)]
    [InlineData("di1ei2ee", BencodeParseErrorKind.InvalidDictionaryKey, 1)]
    [InlineData("d1:bi1e1:ai2ee", BencodeParseErrorKind.UnsortedDictionaryKeys, 7)]
    [InlineData("d1:ai1e1:ai2ee", BencodeParseErrorKind.UnsortedDictionaryKeys, 7)]
    public void TryParse_WithInvalidValue_ReturnsExpectedError(string input, BencodeParseErrorKind expectedKind, int expectedOffset)
    {
        var bytes = GetBytes(input);
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeFalse();
        value.Should().BeNull();
        bytesConsumed.Should().Be(0);
        error.Kind.Should().Be(expectedKind);
        error.Offset.Should().Be(expectedOffset);
        error.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void TryParse_WithExcessiveNesting_ReturnsMaxDepthExceeded()
    {
        var input = new string('l', 130) + new string('e', 130);
        var bytes = GetBytes(input);
        var result = BencodeValueParser.TryParse(bytes, out var value, out var bytesConsumed, out var error);

        result.Should().BeFalse();
        value.Should().BeNull();
        bytesConsumed.Should().Be(0);
        error.Kind.Should().Be(BencodeParseErrorKind.MaxDepthExceeded);
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