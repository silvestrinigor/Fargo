using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserGroupTests
{
    private static UserGroup CreateUserGroup()
    {
        return new UserGroup
        {
            Nameid = (Nameid)"admins"
        };
    }

    [Fact]
    public void IsActive_Should_BeTrue_ByDefault()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var result = userGroup.IsActive;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Activate_Should_SetIsActive_ToTrue()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.Deactivate();

        // Act
        userGroup.Activate();

        // Assert
        Assert.True(userGroup.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActive_ToFalse()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        userGroup.Deactivate();

        // Assert
        Assert.False(userGroup.IsActive);
    }

    [Fact]
    public void ValidateIsActive_Should_NotThrow_When_GroupIsActive()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var exception = Record.Exception(userGroup.ValidateIsActive);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateIsActive_Should_ThrowUserGroupInactiveFargoDomainException_When_GroupIsInactive()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.Deactivate();

        // Act
        var exception = Assert.Throws<UserGroupInactiveFargoDomainException>(
            userGroup.ValidateIsActive);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(exception.UserGroupGuid, userGroup.Guid);
    }

    [Fact]
    public void AddPermission_Should_AddPermission_When_ItDoesNotExist()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        userGroup.AddPermission(ActionType.CreateArticle);

        // Assert
        Assert.Single(userGroup.UserGroupPermissions);
        Assert.Contains(
            userGroup.UserGroupPermissions,
            x => x.Action == ActionType.CreateArticle);
    }

    [Fact]
    public void AddPermission_Should_NotAddDuplicatePermission_When_ItAlreadyExists()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);

        // Act
        userGroup.AddPermission(ActionType.CreateArticle);

        // Assert
        Assert.Single(userGroup.UserGroupPermissions);
        Assert.Equal(
            ActionType.CreateArticle,
            userGroup.UserGroupPermissions.Single().Action);
    }

    [Fact]
    public void AddPermission_Should_AssignCurrentUserGroup_ToCreatedPermission()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        userGroup.AddPermission(ActionType.CreateArticle);

        // Assert
        var permission = Assert.Single(userGroup.UserGroupPermissions);
        Assert.Same(userGroup, permission.UserGroup);
    }

    [Fact]
    public void RemovePermission_Should_RemovePermission_When_ItExists()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);

        // Act
        userGroup.RemovePermission(ActionType.CreateArticle);

        // Assert
        Assert.Empty(userGroup.UserGroupPermissions);
    }

    [Fact]
    public void RemovePermission_Should_DoNothing_When_ItDoesNotExist()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);

        // Act
        userGroup.RemovePermission(ActionType.DeleteArticle);

        // Assert
        Assert.Single(userGroup.UserGroupPermissions);
        Assert.Contains(
            userGroup.UserGroupPermissions,
            x => x.Action == ActionType.CreateArticle);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_PermissionExists()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);

        // Act
        var result = userGroup.HasPermission(ActionType.CreateArticle);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_PermissionDoesNotExist()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var result = userGroup.HasPermission(ActionType.CreateArticle);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Description_Should_BeEmpty_ByDefault()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var result = userGroup.Description;

        // Assert
        Assert.Equal(Description.Empty, result);
    }
}