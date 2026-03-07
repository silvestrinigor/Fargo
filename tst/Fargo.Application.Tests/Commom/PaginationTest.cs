using Fargo.Application.Commom;

namespace Fargo.Application.Tests.Commom;

public sealed class PaginationTests
{
    [Fact]
    public void Constructor_Should_UseDefaultValues_When_NoArgumentsProvided()
    {
        // Act
        var pagination = new Pagination();

        // Assert
        Assert.Equal(Page.DefaultValue, pagination.Page.Value);
        Assert.Equal(Limit.DefaultValue, pagination.Limit.Value);
    }

    [Fact]
    public void Constructor_Should_SetPageAndLimit_When_Provided()
    {
        // Arrange
        var page = new Page(3);
        var limit = new Limit(50);

        // Act
        var pagination = new Pagination(page, limit);

        // Assert
        Assert.Equal(3, pagination.Page.Value);
        Assert.Equal(50, pagination.Limit.Value);
    }

    [Fact]
    public void Skip_Should_ReturnZero_When_PageIsOne()
    {
        // Arrange
        var pagination = new Pagination(new Page(1), new Limit(20));

        // Act
        var result = pagination.Skip;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Skip_Should_CalculateCorrectValue()
    {
        // Arrange
        var pagination = new Pagination(new Page(3), new Limit(10));

        // Act
        var result = pagination.Skip;

        // Assert
        Assert.Equal(20, result);
    }

    [Fact]
    public void Take_Should_ReturnLimitValue()
    {
        // Arrange
        var pagination = new Pagination(new Page(2), new Limit(25));

        // Act
        var result = pagination.Take;

        // Assert
        Assert.Equal(25, result);
    }

    [Fact]
    public void Skip_Should_WorkWithDefaultLimit()
    {
        // Arrange
        var pagination = new Pagination(new Page(2));

        // Act
        var result = pagination.Skip;

        // Assert
        Assert.Equal(Limit.DefaultValue, pagination.Take);
        Assert.Equal((2 - 1) * Limit.DefaultValue, result);
    }

    [Fact]
    public void Take_Should_WorkWithDefaultLimit()
    {
        // Arrange
        var pagination = new Pagination();

        // Act
        var result = pagination.Take;

        // Assert
        Assert.Equal(Limit.DefaultValue, result);
    }

    [Fact]
    public void Skip_Should_WorkWithDefaultPage()
    {
        // Arrange
        var pagination = new Pagination(limit: new Limit(30));

        // Act
        var result = pagination.Skip;

        // Assert
        Assert.Equal(0, result);
        Assert.Equal(30, pagination.Take);
    }

    [Fact]
    public void RecordStructEquality_Should_Work()
    {
        // Arrange
        var p1 = new Pagination(new Page(2), new Limit(10));
        var p2 = new Pagination(new Page(2), new Limit(10));

        // Act
        var result = p1 == p2;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RecordStructEquality_Should_BeFalse_WhenValuesDiffer()
    {
        // Arrange
        var p1 = new Pagination(new Page(2), new Limit(10));
        var p2 = new Pagination(new Page(3), new Limit(10));

        // Act
        var result = p1 == p2;

        // Assert
        Assert.False(result);
    }
}