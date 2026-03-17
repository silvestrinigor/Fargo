using Fargo.Domain.Security;

namespace Fargo.Domain.Tests.Security;

public sealed class SystemActorTests
{
    [Fact]
    public void Guid_Should_HaveExpectedPredefinedValue()
    {
        // Arrange
        var expected = new Guid("00000000-0000-0000-0000-000000000001");

        // Act
        var result = SystemActor.Guid;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Guid_Should_NotBeEmpty()
    {
        // Act
        var result = SystemActor.Guid;

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void Guid_Should_BeStableAcrossMultipleAccesses()
    {
        // Act
        var first = SystemActor.Guid;
        var second = SystemActor.Guid;

        // Assert
        Assert.Equal(first, second);
    }
}
