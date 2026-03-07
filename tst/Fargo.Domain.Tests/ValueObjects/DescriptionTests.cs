using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.ValueObjects;

public sealed class DescriptionTests
{
    [Fact]
    public void Constructor_Should_CreateDescription_When_ValueIsValid()
    {
        // Arrange
        var value = "Valid description";

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void DefaultConstructor_Should_CreateEmptyDescription()
    {
        // Act
        var description = new Description();

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }

    [Fact]
    public void EmptyProperty_Should_ReturnEmptyDescription()
    {
        // Act
        var description = Description.Empty;

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }

    [Fact]
    public void FromString_Should_CreateDescription()
    {
        // Arrange
        var value = "Some description";

        // Act
        var description = Description.FromString(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void ToString_Should_ReturnUnderlyingValue()
    {
        // Arrange
        var value = "Example description";
        var description = new Description(value);

        // Act
        var result = description.ToString();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnString()
    {
        // Arrange
        var description = new Description("Example");

        // Act
        string result = description;

        // Assert
        Assert.Equal("Example", result);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateDescription()
    {
        // Arrange
        var value = "Example";

        // Act
        var description = (Description)value;

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void DefaultStruct_Should_ReturnEmptyString()
    {
        // Arrange
        Description description = default;

        // Act
        var result = description.Value;

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_ValueIsNull()
    {
        // Act
        void act() => _ = new Description((string)null!);

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(act);
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsTooLong()
    {
        // Arrange
        var value = new string('a', Description.MaxLength + 1);

        // Act
        void act() => _ = new Description(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumLength()
    {
        // Arrange
        var value = new string('a', Description.MaxLength);

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(value, description.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptEmptyString()
    {
        // Arrange
        var value = "";

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }
}