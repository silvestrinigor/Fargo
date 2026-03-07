using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class NameidTests
{
    [Fact]
    public void Constructor_Should_CreateNameid_When_ValueIsValid()
    {
        // Arrange
        var value = "igor.silvestrin";

        // Act
        var nameid = new Nameid(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void FromString_Should_CreateNameid_When_ValueIsValid()
    {
        // Arrange
        var value = "igor_silvestrin";

        // Act
        var nameid = Nameid.FromString(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void NewNameid_Should_CreateNameid_When_ValueIsValid()
    {
        // Arrange
        var value = "igor-silvestrin";

        // Act
        var nameid = Nameid.NewNameid(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void ToString_Should_ReturnValue()
    {
        // Arrange
        var value = "igor123";
        var nameid = new Nameid(value);

        // Act
        var result = nameid.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnStringValue()
    {
        // Arrange
        var nameid = new Nameid("igor123");

        // Act
        string value = nameid;

        // Assert
        Assert.Equal("igor123", value);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateNameid()
    {
        // Arrange
        var value = "igor123";

        // Act
        var nameid = (Nameid)value;

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Nameid nameid = default;

        // Act
        void act() => _ = nameid.Value;

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("Nameid not initialized.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_Should_ThrowArgumentException_When_ValueIsNullOrWhitespace(string? value)
    {
        // Act
        void act() => _ = new Nameid((string)value!);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooShort()
    {
        // Arrange
        var value = "ab";

        // Act
        void act() => _ = new Nameid(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('a', Nameid.MaxLength + 1);

        // Act
        void act() => _ = new Nameid(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueStartsWithWhitespace()
    {
        // Arrange
        var value = " igor";

        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueEndsWithWhitespace()
    {
        // Arrange
        var value = "igor ";

        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsSpaces()
    {
        // Arrange
        var value = "igor silvestrin";

        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData(".igor")]
    [InlineData("_igor")]
    [InlineData("-igor")]
    public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotStartWithLetterOrDigit(string value)
    {
        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("igor.")]
    [InlineData("igor_")]
    [InlineData("igor-")]
    public void Constructor_Should_ThrowArgumentException_When_ValueDoesNotEndWithLetterOrDigit(string value)
    {
        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("igor@123")]
    [InlineData("igor#123")]
    [InlineData("igor$123")]
    [InlineData("igor!123")]
    [InlineData("igor+123")]
    [InlineData("igor/123")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsInvalidCharacter(string value)
    {
        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("igor..silvestrin")]
    [InlineData("igor__silvestrin")]
    [InlineData("igor--silvestrin")]
    [InlineData("igor.-silvestrin")]
    [InlineData("igor_.silvestrin")]
    [InlineData("igor-_silvestrin")]
    public void Constructor_Should_ThrowArgumentException_When_ValueContainsConsecutiveSpecialCharacters(string value)
    {
        // Act
        void act() => _ = new Nameid(value);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("igor")]
    [InlineData("igor123")]
    [InlineData("igor.silvestrin")]
    [InlineData("igor_silvestrin")]
    [InlineData("igor-silvestrin")]
    [InlineData("i.g-o_r1")]
    public void Constructor_Should_AcceptValidValues(string value)
    {
        // Act
        var nameid = new Nameid(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMinimumLength()
    {
        // Arrange
        var value = new string('a', Nameid.MinLength);

        // Act
        var nameid = new Nameid(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumLength()
    {
        // Arrange
        var value = new string('a', Nameid.MaxLength);

        // Act
        var nameid = new Nameid(value);

        // Assert
        Assert.Equal(value, nameid.Value);
    }
}