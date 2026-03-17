using Fargo.Domain.Security;
using Fargo.Infrastructure.Security;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class SystemCurrentUserTests
{
    [Fact]
    public void IsAuthenticated_Should_ReturnTrue()
    {
        // Arrange
        var currentUser = new SystemCurrentUser();

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UserGuid_Should_ReturnSystemActorGuid()
    {
        // Arrange
        var currentUser = new SystemCurrentUser();

        // Act
        var result = currentUser.UserGuid;

        // Assert
        Assert.Equal(SystemActor.Guid, result);
    }

    [Fact]
    public void UserGuid_Should_NotBeEmpty()
    {
        // Arrange
        var currentUser = new SystemCurrentUser();

        // Act
        var result = currentUser.UserGuid;

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void UserGuid_Should_BeStableAcrossMultipleAccesses()
    {
        // Arrange
        var currentUser = new SystemCurrentUser();

        // Act
        var first = currentUser.UserGuid;
        var second = currentUser.UserGuid;

        // Assert
        Assert.Equal(first, second);
    }
}
