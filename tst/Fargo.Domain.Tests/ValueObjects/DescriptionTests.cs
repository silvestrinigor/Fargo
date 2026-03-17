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
    public void DefaultStruct_Value_Should_ThrowInvalidOperationException()
    {
        // Arrange
        Description description = default;

        // Act
        void act() => _ = description.Value;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void DefaultStruct_ToString_Should_ThrowInvalidOperationException()
    {
        // Arrange
        Description description = default;

        // Act
        void act() => _ = description.ToString();

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void DefaultStruct_ImplicitOperator_Should_ThrowInvalidOperationException()
    {
        // Arrange
        Description description = default;

        // Act
        void act() => _ = (string)description;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
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
        var value = string.Empty;

        // Act
        var description = new Description(value);

        // Assert
        Assert.Equal(string.Empty, description.Value);
    }

    [Fact]
    public void Equals_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Description("First");
        var right = new Description("Second");

        // Act
        var result = left.Equals(right);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnTrue_When_ObjectIsDescriptionWithSameValue()
    {
        // Arrange
        object other = new Description("Same value");
        var description = new Description("Same value");

        // Act
        var result = description.Equals(other);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsNull()
    {
        // Arrange
        var description = new Description("Value");

        // Act
        var result = description.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EqualsObject_Should_ReturnFalse_When_ObjectIsDifferentType()
    {
        // Arrange
        var description = new Description("Value");

        // Act
        var result = description.Equals("Value");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnTrue_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorEqual_Should_ReturnFalse_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Description("First");
        var right = new Description("Second");

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnTrue_When_ValuesAreDifferent()
    {
        // Arrange
        var left = new Description("First");
        var right = new Description("Second");

        // Act
        var result = left != right;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void OperatorNotEqual_Should_ReturnFalse_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var result = left != right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_Should_ReturnSameHash_When_ValuesAreEqual()
    {
        // Arrange
        var left = new Description("Same value");
        var right = new Description("Same value");

        // Act
        var leftHash = left.GetHashCode();
        var rightHash = right.GetHashCode();

        // Assert
        Assert.Equal(leftHash, rightHash);
    }

    [Fact]
    public void DefaultStruct_Should_NotBeEqualTo_EmptyDescription()
    {
        // Arrange
        Description left = default;
        var right = Description.Empty;

        // Act
        var result = left == right;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DefaultStruct_Should_BeEqualTo_DefaultStruct()
    {
        // Arrange
        Description left = default;
        Description right = default;

        // Act
        var result = left == right;

        // Assert
        Assert.True(result);
    }
}
