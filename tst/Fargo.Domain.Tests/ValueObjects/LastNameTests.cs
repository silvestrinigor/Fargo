using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class LastNameTests
{
    [Fact]
    public void Constructor_Should_CreateLastName_When_ValueIsValid()
    {
        // Arrange
        var value = "Silva";

        // Act
        var lastName = new LastName(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void Constructor_Should_TrimValue_When_ValueHasOuterSpaces()
    {
        // Arrange
        var value = "  Silva  ";

        // Act
        var lastName = new LastName(value);

        // Assert
        Assert.Equal("Silva", lastName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateLastName_When_ValueContainsSpace()
    {
        // Arrange
        var value = "De Souza";

        // Act
        var lastName = new LastName(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateLastName_When_ValueContainsHyphen()
    {
        // Arrange
        var value = "Jean-Pierre";

        // Act
        var lastName = new LastName(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void Constructor_Should_CreateLastName_When_ValueContainsUnicodeLetters()
    {
        // Arrange
        var value = "Gonçalves";

        // Act
        var lastName = new LastName(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void FromString_Should_CreateLastName_When_ValueIsValid()
    {
        // Arrange
        var value = "Silva";

        // Act
        var lastName = LastName.FromString(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void NewLastName_Should_CreateLastName_When_ValueIsValid()
    {
        // Arrange
        var value = "Souza";

        // Act
        var lastName = LastName.NewLastName(value);

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = "Silva";
        var lastName = new LastName(value);

        // Act
        var result = lastName.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnString()
    {
        // Arrange
        var value = "Silva";
        var lastName = new LastName(value);

        // Act
        string result = lastName;

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateLastName()
    {
        // Arrange
        var value = "Silva";

        // Act
        var lastName = (LastName)value;

        // Assert
        Assert.Equal(value, lastName.Value);
    }

    [Fact]
    public void EqualityOperator_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new LastName("Silva");
        var right = new LastName("Silva");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void InequalityOperator_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new LastName("Silva");
        var right = new LastName("Souza");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new LastName("Silva");
        var right = new LastName("Silva");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var lastName = new LastName("Silva");

        // Act
        var result = lastName.Equals("Silva");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameValue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new LastName("Silva");
        var right = new LastName("Silva");

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
        void act() => _ = new LastName(value!);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVW")]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_LengthIsInvalid(string value)
    {
        // Act
        void act() => _ = new LastName(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Theory]
    [InlineData("Silva1")]
    [InlineData("Souza!")]
    [InlineData("De_Souza")]
    [InlineData("Smith.")]
    [InlineData("Jean@Pierre")]
    [InlineData("O'Connor")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsInvalidCharacters(string value)
    {
        // Act
        void act() => _ = new LastName(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("-Silva")]
    [InlineData("Silva-")]
    [InlineData("De  Souza")]
    [InlineData("Jean--Pierre")]
    [InlineData("De -Souza")]
    [InlineData("De- Souza")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsInvalidSeparators(string value)
    {
        // Act
        void act() => _ = new LastName(value);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}
