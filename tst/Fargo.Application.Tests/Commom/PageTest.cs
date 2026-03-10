using Fargo.Application.Commom;

namespace Fargo.Application.Tests.Commom;

public sealed class PageTests
{
    [Fact]
    public void FirstPage_Should_ReturnPageWithValueOne()
    {
        // Act
        var page = Page.FirstPage;

        // Assert
        Assert.Equal(1, page.Value);
    }

    [Fact]
    public void Constructor_Should_CreatePage_When_ValueIsValid()
    {
        // Arrange
        var value = 5;

        // Act
        var page = new Page(value);

        // Assert
        Assert.Equal(value, page.Value);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentOutOfRangeException_When_ValueIsLessThanMin()
    {
        // Arrange
        var value = Page.MinValue - 1;

        // Act
        void act() => _ = new Page(value);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Should_AcceptMinimumValue()
    {
        // Arrange
        var value = Page.MinValue;

        // Act
        var page = new Page(value);

        // Assert
        Assert.Equal(value, page.Value);
    }

    [Fact]
    public void Constructor_Should_AcceptMaximumValue()
    {
        // Arrange
        var value = Page.MaxValue;

        // Act
        var page = new Page(value);

        // Assert
        Assert.Equal(value, page.Value);
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Page page = default;

        // Act
        void act() => _ = page.Value;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ImplicitOperator_Should_ReturnIntValue()
    {
        // Arrange
        var page = new Page(7);

        // Act
        int result = page;

        // Assert
        Assert.Equal(7, result);
    }

    [Fact]
    public void ImplicitOperator_Should_ThrowInvalidOperationException_When_DefaultStructIsUsed()
    {
        // Arrange
        Page page = default;

        // Act
        void act() => _ = (int)page;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ExplicitOperator_Should_CreatePage()
    {
        // Arrange
        var value = 9;

        // Act
        var page = (Page)value;

        // Assert
        Assert.Equal(value, page.Value);
    }

    [Fact]
    public void Parse_Should_ReturnPage_When_ValueIsValid()
    {
        // Arrange
        var value = "12";

        // Act
        var page = Page.Parse(value, null);

        // Assert
        Assert.Equal(12, page.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public void Parse_Should_ThrowFormatException_When_ValueIsInvalid(string? value)
    {
        // Act
        void act() => _ = Page.Parse(value!, null);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("25", 25)]
    [InlineData("2147483647", 2147483647)]
    public void TryParse_Should_ReturnTrue_When_ValueIsValid(string input, int expected)
    {
        // Act
        var result = Page.TryParse(input, null, out var page);

        // Assert
        Assert.True(result);
        Assert.Equal(expected, page.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public void TryParse_Should_ReturnFalse_When_ValueIsInvalid(string? input)
    {
        // Act
        var result = Page.TryParse(input, null, out var page);

        // Assert
        Assert.False(result);
        Assert.Equal(default(Page), page);
    }

    [Fact]
    public void TryParse_Should_RespectFormatProvider()
    {
        // Arrange
        var culture = new System.Globalization.CultureInfo("en-US");

        // Act
        var result = Page.TryParse("123", culture, out var page);

        // Assert
        Assert.True(result);
        Assert.Equal(123, page.Value);
    }
}