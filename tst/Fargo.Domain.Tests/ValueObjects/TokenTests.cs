using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class TokenTests
{
    [Fact]
    public void Constructor_Should_CreateToken_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', Token.MinLength);

        // Act
        var token = new Token(value);

        // Assert
        Assert.Equal(value, token.Value);
    }

    [Fact]
    public void FromString_Should_CreateToken_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', Token.MinLength);

        // Act
        var token = Token.FromString(value);

        // Assert
        Assert.Equal(value, token.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = new string('a', Token.MinLength);
        var token = new Token(value);

        // Act
        var result = token.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnStringValue()
    {
        // Arrange
        var value = new string('a', Token.MinLength);
        var token = new Token(value);

        // Act
        string result = token;

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateToken_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', Token.MinLength);

        // Act
        var token = (Token)value;

        // Assert
        Assert.Equal(value, token.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Token token = default;

        // Act
        void act() => _ = token.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Token value must be set.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new Token((string)value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
    {
        // Arrange
        var value = new string('a', Token.MinLength - 1);

        // Act
        void act() => _ = new Token(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('a', Token.MaxLength + 1);

        // Act
        void act() => _ = new Token(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa ")]
    [InlineData(" aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaa\taaaaaaaaaaaaaaaaaaaaaaaa")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsWhitespace(string value)
    {
        // Act
        void act() => _ = new Token(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_AcceptMinimumLength()
    {
        // Arrange
        var value = new string('a', Token.MinLength);

        // Act
        var token = new Token(value);

        // Assert
        Assert.Equal(value, token.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumLength()
    {
        // Arrange
        var value = new string('a', Token.MaxLength);

        // Act
        var token = new Token(value);

        // Assert
        Assert.Equal(value, token.Value);
    }
}