using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class FirstNameTests
{
    [Fact]
    public void Constructor_Should_CreateFirstName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var firstName = new FirstName(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void Constructor_Should_TrimValue_When_ValueHasOuterSpaces()
    {
        // Arrange
        var value = "  Igor  ";

        // Act
        var firstName = new FirstName(value);

        // Assert
        Assert.Equal("Igor", firstName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateFirstName_When_ValueContainsSpace()
    {
        // Arrange
        var value = "Ana Maria";

        // Act
        var firstName = new FirstName(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateFirstName_When_ValueContainsHyphen()
    {
        // Arrange
        var value = "Jean-Paul";

        // Act
        var firstName = new FirstName(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateFirstName_When_ValueContainsUnicodeLetters()
    {
        // Arrange
        var value = "João";

        // Act
        var firstName = new FirstName(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void FromString_Should_CreateFirstName_When_ValueIsValid()
    {
        // Arrange
        var value = "Igor";

        // Act
        var firstName = FirstName.FromString(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void NewFirstName_Should_CreateFirstName_When_ValueIsValid()
    {
        // Arrange
        var value = "Maria";

        // Act
        var firstName = FirstName.NewFirstName(value);

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = "Igor";
        var firstName = new FirstName(value);

        // Act
        var result = firstName.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnString()
    {
        // Arrange
        var value = "Igor";
        var firstName = new FirstName(value);

        // Act
        string result = firstName;

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateFirstName()
    {
        // Arrange
        var value = "Igor";

        // Act
        var firstName = (FirstName)value;

        // Assert
        Assert.Equal(value, firstName.Value);
    }

    [Fact]
    public void EqualityOperator_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new FirstName("Igor");
        var right = new FirstName("Igor");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new FirstName("Igor");
        var right = new FirstName("Maria");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new FirstName("Igor");
        var right = new FirstName("Igor");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var firstName = new FirstName("Igor");

        // Act
        var result = firstName.Equals("Igor");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new FirstName("Igor");
        var right = new FirstName("Igor");

        // Act
        var leftHashCode = left.GetHashCode();
        var rightHashCode = right.GetHashCode();

        // Assert
        Assert.Equal(leftHashCode, rightHashCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhiteSpace(string? value)
    {
        // Act
        void act() => _ = new FirstName(value!);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_LengthIsInvalid(string value)
    {
        // Act
        void act() => _ = new FirstName(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData("Igor1")]
    [InlineData("Maria!")]
    [InlineData("Ana_Maria")]
    [InlineData("John.")]
    [InlineData("Jean@Paul")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsInvalidCharacters(string value)
    {
        // Act
        void act() => _ = new FirstName(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("-Igor")]
    [InlineData("Igor-")]
    [InlineData("Ana  Maria")]
    [InlineData("Ana--Maria")]
    [InlineData("Ana -Maria")]
    [InlineData("Ana- Maria")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsInvalidSeparators(string value)
    {
        // Act
        void act() => _ = new FirstName(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}