using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void Description_Should_DefaultTo_Empty()
    {
        // Act
        var user = CreateUser();

        // Assert
        Assert.Equal(Description.Empty, user.Description);
    }

    [Fact]
    public void UserPermissions_Should_BeEmpty_ByDefault()
    {
        // Act
        var user = CreateUser();

        // Assert
        Assert.Empty(user.UserPermissions);
    }

    [Fact]
    public void AddPermission_Should_AddPermission_When_NotAlreadyPresent()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.AddPermission(ActionType.CreateArticle);

        // Assert
        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(ActionType.CreateArticle, permission.Action);
        Assert.Same(user, permission.User);
    }

    [Fact]
    public void AddPermission_Should_NotAddDuplicatePermission_When_AlreadyPresent()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateArticle);

        // Act
        user.AddPermission(ActionType.CreateArticle);

        // Assert
        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(ActionType.CreateArticle, permission.Action);
    }

    [Fact]
    public void AddPermission_Should_AddMultipleDifferentPermissions()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.AddPermission(ActionType.CreateArticle);
        user.AddPermission(ActionType.DeleteArticle);

        // Assert
        Assert.Equal(2, user.UserPermissions.Count);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateArticle);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.DeleteArticle);
    }

    [Fact]
    public void RemovePermission_Should_RemovePermission_When_ItExists()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateArticle);
        user.AddPermission(ActionType.DeleteArticle);

        // Act
        user.RemovePermission(ActionType.CreateArticle);

        // Assert
        Assert.Single(user.UserPermissions);
        Assert.DoesNotContain(user.UserPermissions, x => x.Action == ActionType.CreateArticle);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.DeleteArticle);
    }

    [Fact]
    public void RemovePermission_Should_DoNothing_When_PermissionDoesNotExist()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateArticle);

        // Act
        user.RemovePermission(ActionType.DeleteArticle);

        // Assert
        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(ActionType.CreateArticle, permission.Action);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasPermission()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateArticle);

        // Act
        var result = user.HasPermission(ActionType.CreateArticle);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_UserDoesNotHavePermission()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = user.HasPermission(ActionType.CreateArticle);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidatePermission_Should_NotThrow_When_UserHasPermission()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateArticle);

        // Act
        var exception = Record.Exception(() => user.ValidatePermission(ActionType.CreateArticle));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePermission_Should_Throw_When_UserDoesNotHavePermission()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.ValidatePermission(ActionType.CreateArticle);

        // Assert
        Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
    }

    private static User CreateUser(IReadOnlyCollection<UserPermission>? userPermissions = null)
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("igor123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength)),
            UserPermissions = userPermissions ?? []
        };
    }
}