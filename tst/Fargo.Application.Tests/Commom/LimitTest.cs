using Fargo.Application.Common;

namespace Fargo.Application.Tests.Commom;

public sealed class LimitTests
{
    [Fact]
    public void MaxLimit_Should_ReturnLimitWithMaxValue()
    {
        // Act
        var limit = Limit.MaxLimit;

        // Assert
        Assert.Equal(Limit.MaxValue, limit.Value);
    }

    [Fact]
    public void Constructor_Should_CreateLimit_When_ValueIsValid()
    {
        // Arrange
        var value = 50;

        // Act
        var limit = new Limit(value);

        // Assert
        Assert.Equal(value, limit.Value);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsLessThanMin()
    {
        // Arrange
        var value = Limit.MinValue - 1;

        // Act
        void act() => _ = new Limit(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsGreaterThanMax()
    {
        // Arrange
        var value = Limit.MaxValue + 1;

        // Act
        void act() => _ = new Limit(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Limit limit = default;

        // Act
        void act() => _ = limit.Value;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnIntValue()
    {
        // Arrange
        var limit = new Limit(42);

        // Act
        int result = limit;

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Limit limit = default;

        // Act
        void act() => _ = (int)limit;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ExplicitOperator_Should_CreateLimit()
    {
        // Arrange
        var value = 30;

        // Act
        var limit = (Limit)value;

        // Assert
        Assert.Equal(value, limit.Value);
    }

    [Fact]
    public void Parse_Should_ReturnLimit_When_StringIsValid()
    {
        // Arrange
        var value = "25";

        // Act
        var limit = Limit.Parse(value, null);

        // Assert
        Assert.Equal(25, limit.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1001")]
    public void Parse_Should_ThrowFormatException_When_ValueIsInvalid(string? value)
    {
        // Act
        void act() => Limit.Parse(value!, null);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Theory]
    [InlineData("10")]
    [InlineData("500")]
    [InlineData("1000")]
    public void TryParse_Should_ReturnTrue_When_ValueIsValid(string input)
    {
        // Act
        var result = Limit.TryParse(input, null, out var limit);

        // Assert
        Assert.True(result);
        Assert.Equal(int.Parse(input), limit.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public void TryParse_Should_ReturnFalse_When_ValueIsInvalid(string? input)
    {
        // Act
        var result = Limit.TryParse(input, null, out var limit);

        // Assert
        Assert.False(result);
        Assert.Equal(default, limit);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1001")]
    public void TryParse_Should_ReturnFalse_When_ValueIsOutsideRange(string input)
    {
        // Act
        var result = Limit.TryParse(input, null, out var limit);

        // Assert
        Assert.False(result);
        Assert.Equal(default, limit);
    }

    [Fact]
    public void TryParse_Should_RespectFormatProvider()
    {
        // Arrange
        var culture = new System.Globalization.CultureInfo("en-US");

        // Act
        var result = Limit.TryParse("123", culture, out var limit);

        // Assert
        Assert.True(result);
        Assert.Equal(123, limit.Value);
    }
}