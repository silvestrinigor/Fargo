using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class PasswordHashTests
{
    [Fact]
    public void Constructor_Should_CreateHash_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);

        // Act
        var hash = new PasswordHash(value);

        // Assert
        Assert.Equal(value, hash.Value);
    }

    [Fact]
    public void FromString_Should_CreateHash_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);

        // Act
        var hash = PasswordHash.FromString(value);

        // Assert
        Assert.Equal(value, hash.Value);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnString()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);
        var hash = new PasswordHash(value);

        // Act
        string result = hash;

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateHash()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);

        // Act
        var hash = (PasswordHash)value;

        // Assert
        Assert.Equal(value, hash.Value);
    }

    [Fact]
    public void ToString_Should_ReturnHashValue()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);
        var hash = new PasswordHash(value);

        // Act
        var result = hash.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Value_Should_Throw_When_DefaultStructIsUsed()
    {
        // Arrange
        PasswordHash hash = default;

        // Act
        void act() => _ = hash.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Password hash value must be set.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_NullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new PasswordHash((string)value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRange_When_TooShort()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength - 1);

        // Act
        void act() => _ = new PasswordHash(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRange_When_TooLong()
    {
        // Arrange
        var value = new string('a', PasswordHash.MaxLength + 1);

        // Act
        void act() => _ = new PasswordHash(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ContainsWhitespace()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength - 2) + " a";

        // Act
        void act() => _ = new PasswordHash(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_Accept_MinLength()
    {
        // Arrange
        var value = new string('a', PasswordHash.MinLength);

        // Act
        var hash = new PasswordHash(value);

        // Assert
        Assert.Equal(value, hash.Value);
    }

    [Fact]
    public void Constructor_Should_Accept_MaxLength()
    {
        // Arrange
        var value = new string('a', PasswordHash.MaxLength);

        // Act
        var hash = new PasswordHash(value);

        // Assert
        Assert.Equal(value, hash.Value);
    }
}