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
}