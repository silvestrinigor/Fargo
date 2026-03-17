using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class PasswordTests
{
    [Fact]
    public void Constructor_Should_CreatePassword_When_ValueIsValid()
    {
        // Arrange
        var value = "Secure@123";

        // Act
        var password = new Password(value);

        // Assert
        Assert.Equal(value, password.Value);
    }

    [Fact]
    public void FromString_Should_CreatePassword_When_ValueIsValid()
    {
        // Arrange
        var value = "Secure@123";

        // Act
        var password = Password.FromString(value);

        // Assert
        Assert.Equal(value, password.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = "Secure@123";
        var password = new Password(value);

        // Act
        var result = password.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnStringValue()
    {
        // Arrange
        var password = new Password("Secure@123");

        // Act
        string value = password;

        // Assert
        Assert.Equal("Secure@123", value);
    }

    [Fact]
    public void ExplicitOperator_Should_CreatePassword_When_ValueIsValid()
    {
        // Arrange
        var value = "Secure@123";

        // Act
        var password = (Password)value;

        // Assert
        Assert.Equal(value, password.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Password password = default;

        // Act
        void act() => _ = password.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Password value must be set.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new Password(value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
    {
        // Arrange
        var value = "A@1bcdef";

        // Act
        void act() => _ = new Password(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('A', Password.MaxLength + 1) + "1!";

        // Act
        void act() => _ = new Password(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsSpaces()
    {
        // Arrange
        var value = "Secure @123";

        // Act
        void act() => _ = new Password(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainLetter()
    {
        // Arrange
        var value = "12345678!";

        // Act
        void act() => _ = new Password(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainDigit()
    {
        // Arrange
        var value = "Password!";

        // Act
        void act() => _ = new Password(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotContainSpecialCharacter()
    {
        // Arrange
        var value = "Password1";

        // Act
        void act() => _ = new Password(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@123");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@456");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectIsPasswordWithSameValue()
    {
        // Arrange
        var password = new Password("Secure@123");
        object other = new Password("Secure@123");

        // Act
        var result = password.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var password = new Password("Secure@123");

        // Act
        var result = password.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var password = new Password("Secure@123");

        // Act
        var result = password.Equals("Secure@123");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@123");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@456");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@456");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@123");

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Password("Secure@123");
        var right = new Password("Secure@123");

        // Act
        var leftHash = left.GetHashCode();
        var rightHash = right.GetHashCode();

        // Assert
        Assert.Equal(leftHash, rightHash);
    }
}
