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

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('b', PasswordHash.MinLength));

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectIsPasswordHashWithSameValue()
    {
        // Arrange
        var hash = new PasswordHash(new string('a', PasswordHash.MinLength));
        object other = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = hash.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var hash = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = hash.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var hash = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = hash.Equals(new string('a', PasswordHash.MinLength));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('b', PasswordHash.MinLength));

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('b', PasswordHash.MinLength));

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesAreEqual()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('a', PasswordHash.MinLength));

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesAreEqual()
    {
        // Arrange
        var left = new PasswordHash(new string('a', PasswordHash.MinLength));
        var right = new PasswordHash(new string('a', PasswordHash.MinLength));

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
        PasswordHash left = default;
        PasswordHash right = default;

        // Act
        var exception = Record.Exception(() => left.Equals(right));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OperatorEqual_Should_NotThrow_When_BothAreDefault()
    {
        // Arrange
        PasswordHash left = default;
        PasswordHash right = default;

        // Act
        var exception = Record.Exception(() => _ = left == right);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void GetHashCode_Should_NotThrow_When_DefaultStructIsUsed()
    {
        // Arrange
        PasswordHash hash = default;

        // Act
        var exception = Record.Exception(() => _ = hash.GetHashCode());

        // Assert
        Assert.Null(exception);
    }
}
