using Fargo.Application.Common;

namespace Fargo.Application.Tests.Commom;

public sealed class PaginationTests
{
    [Fact]
    public void First20Pages_Should_ReturnPaginationWithPageOneAndLimitTwenty()
    {
        // Act
        var pagination = Pagination.First20Pages;

        // Assert
        Assert.Equal(1, pagination.Page.Value);
        Assert.Equal(20, pagination.Limit.Value);
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
    public void Constructor_Should_AllowDefaultValues_When_NoArgumentsProvided()
    {
        // Act
        var pagination = new Pagination();

        // Assert
        Assert.Equal(default(Page), pagination.Page);
        Assert.Equal(default(Limit), pagination.Limit);
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
    public void Take_Should_ThrowInvalidOperationException_When_LimitIsDefault()
    {
        // Arrange
        var pagination = new Pagination();

        // Act
        void act() => _ = pagination.Take;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Skip_Should_ThrowInvalidOperationException_When_PageAndLimitAreDefault()
    {
        // Arrange
        var pagination = new Pagination();

        // Act
        void act() => _ = pagination.Skip;

        // Assert
        Assert.Throws<InvalidOperationException>(act);
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