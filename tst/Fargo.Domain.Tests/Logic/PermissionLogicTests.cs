using Fargo.Domain.Enums;
using Fargo.Domain.Logics;

namespace Fargo.Domain.Tests.Logics;

public sealed class PermissionLogicTests
{
    private sealed record TestPermission(ActionType Action) : IPermission;

    private sealed class TestUserWithPermissions(
        IReadOnlyCollection<IPermission> permissions
        ) : IUserWithPermissions
    {
        public IReadOnlyCollection<IPermission> Permissions { get; } = permissions;
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasSpecifiedPermission()
    {
        // Arrange
        var user = new TestUserWithPermissions(
        [
            new TestPermission(ActionType.Create),
            new TestPermission(ActionType.Read)
        ]);

        // Act
        var result = user.HasPermission(ActionType.Create);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_UserDoesNotHaveSpecifiedPermission()
    {
        // Arrange
        var user = new TestUserWithPermissions(
        [
            new TestPermission(ActionType.Read),
            new TestPermission(ActionType.Update)
        ]);

        // Act
        var result = user.HasPermission(ActionType.Delete);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_UserHasNoPermissions()
    {
        // Arrange
        var user = new TestUserWithPermissions([]);

        // Act
        var result = user.HasPermission(ActionType.Read);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ThrowArgumentNullException_When_UserIsNull()
    {
        // Arrange
        IUserWithPermissions? user = null;

        // Act
        var action = () => user!.HasPermission(ActionType.Read);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasDuplicatedMatchingPermissions()
    {
        // Arrange
        var user = new TestUserWithPermissions(
        [
            new TestPermission(ActionType.Read),
            new TestPermission(ActionType.Read)
        ]);

        // Act
        var result = user.HasPermission(ActionType.Read);

        // Assert
        Assert.True(result);
    }
}