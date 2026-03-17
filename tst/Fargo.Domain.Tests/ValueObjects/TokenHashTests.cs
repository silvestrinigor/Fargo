using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class TokenHashTests
{
    [Fact]
    public void Constructor_Should_CreateTokenHash_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);

        // Act
        var tokenHash = new TokenHash(value);

        // Assert
        Assert.Equal(value.ToUpperInvariant(), tokenHash.Value);
    }

    [Fact]
    public void FromString_Should_CreateTokenHash_When_ValueIsValid()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);

        // Act
        var tokenHash = TokenHash.FromString(value);

        // Assert
        Assert.Equal(value.ToUpperInvariant(), tokenHash.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);
        var tokenHash = new TokenHash(value);

        // Act
        var result = tokenHash.ToString();

        // Assert
        Assert.Equal(value.ToUpperInvariant(), result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnString()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);
        var tokenHash = new TokenHash(value);

        // Act
        string result = tokenHash;

        // Assert
        Assert.Equal(value.ToUpperInvariant(), result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateTokenHash()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);

        // Act
        var tokenHash = (TokenHash)value;

        // Assert
        Assert.Equal(value.ToUpperInvariant(), tokenHash.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        TokenHash tokenHash = default;

        // Act
        void act() => _ = tokenHash.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Token hash value must be set.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new TokenHash((string)value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength - 1);

        // Act
        void act() => _ = new TokenHash(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('a', TokenHash.MaxLength + 1);

        // Act
        void act() => _ = new TokenHash(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaa aaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaa\taaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData(" aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsWhitespace(string value)
    {
        // Act
        void act() => _ = new TokenHash(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_AcceptMinimumLength()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);

        // Act
        var tokenHash = new TokenHash(value);

        // Assert
        Assert.Equal(value.ToUpperInvariant(), tokenHash.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumLength()
    {
        // Arrange
        var value = new string('a', TokenHash.MaxLength);

        // Act
        var tokenHash = new TokenHash(value);

        // Assert
        Assert.Equal(value.ToUpperInvariant(), tokenHash.Value);
    }

    [Fact]
    public void Constructor_Should_NormalizeValueToUpperCase()
    {
        // Arrange
        var value = new string('a', TokenHash.MinLength);

        // Act
        var tokenHash = new TokenHash(value);

        // Assert
        Assert.Equal(new string('A', TokenHash.MinLength), tokenHash.Value);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('A', TokenHash.MinLength));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('b', TokenHash.MinLength));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectIsTokenHashWithSameValue()
    {
        // Arrange
        var tokenHash = new TokenHash(new string('a', TokenHash.MinLength));
        object other = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = tokenHash.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectDiffersOnlyByCase()
    {
        // Arrange
        var tokenHash = new TokenHash(new string('a', TokenHash.MinLength));
        object other = new TokenHash(new string('A', TokenHash.MinLength));

        // Act
        var result = tokenHash.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var tokenHash = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = tokenHash.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var tokenHash = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = tokenHash.Equals(new string('a', TokenHash.MinLength));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('A', TokenHash.MinLength));

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('b', TokenHash.MinLength));

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('b', TokenHash.MinLength));

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesAreEqual()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('A', TokenHash.MinLength));

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesAreEqual()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('a', TokenHash.MinLength));

        // Act
        var leftHash = left.GetHashCode();
        var rightHash = right.GetHashCode();

        // Assert
        Assert.Equal(leftHash, rightHash);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesDifferOnlyByCase()
    {
        // Arrange
        var left = new TokenHash(new string('a', TokenHash.MinLength));
        var right = new TokenHash(new string('A', TokenHash.MinLength));

        // Act
        var leftHash = left.GetHashCode();
        var rightHash = right.GetHashCode();

        // Assert
        Assert.Equal(leftHash, rightHash);
    }

    [Fact]
    public void Equals_Should_NotThrow_When_BothAreDefault()
    {
        // Arrange
        TokenHash left = default;
        TokenHash right = default;

        // Act
        var exception = Record.Exception(() => left.Equals(right));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OperatorEqual_Should_NotThrow_When_BothAreDefault()
    {
        // Arrange
        TokenHash left = default;
        TokenHash right = default;

        // Act
        var exception = Record.Exception(() => _ = left == right);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void GetHashCode_Should_NotThrow_When_DefaultStructIsUsed()
    {
        // Arrange
        TokenHash tokenHash = default;

        // Act
        var exception = Record.Exception(() => _ = tokenHash.GetHashCode());

        // Assert
        Assert.Null(exception);
    }
}
