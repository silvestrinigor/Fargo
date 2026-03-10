using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class NameTests
{
    [Fact]
    public void Constructor_Should_CreateName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var name = new Name(value);

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void FromString_Should_CreateName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var name = Name.FromString(value);

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void NewName_Should_CreateName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var name = Name.NewName(value);

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = "Igor";
        var name = new Name(value);

        // Act
        var result = name.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnStringValue()
    {
        // Arrange
        var name = new Name("Igor");

        // Act
        string value = name;

        // Assert
        Assert.Equal("Igor", value);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var name = (Name)value;

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Name name = default;

        // Act
        void act() => _ = name.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Name not initialized.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new Name((string)value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
    {
        // Arrange
        var value = "Ab";

        // Act
        void act() => _ = new Name(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('a', Name.MaxLength + 1);

        // Act
        void act() => _ = new Name(value);

        // Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_AcceptMinimumLength()
    {
        // Arrange
        var value = new string('a', Name.MinLength);

        // Act
        var name = new Name(value);

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumLength()
    {
        // Arrange
        var value = new string('a', Name.MaxLength);

        // Act
        var name = new Name(value);

        // Assert
        Assert.Equal(value, name.Value);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Igor");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Joao");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectIsNameWithSameValue()
    {
        // Arrange
        var name = new Name("Igor");
        object other = new Name("Igor");

        // Act
        var result = name.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var name = new Name("Igor");

        // Act
        var result = name.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var name = new Name("Igor");

        // Act
        var result = name.Equals("Igor");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Igor");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Joao");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Joao");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Igor");

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Name("Igor");
        var right = new Name("Igor");

        // Act
        var leftHash = left.GetHashCode();
        var rightHash = right.GetHashCode();

        // Assert
        Assert.Equal(leftHash, rightHash);
    }
}