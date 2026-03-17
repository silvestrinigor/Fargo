using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserGroupPermissionTests
{
    private static UserGroup CreateUserGroup()
    {
        return new UserGroup
        {
            Guid = Guid.NewGuid(),
            Nameid = (Nameid)"admins"
        };
    }

    [Fact]
    public void UserGroupGuid_Should_BeSynchronized_When_UserGroup_IsAssigned()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var permission = new UserGroupPermission
        {
            UserGroup = userGroup,
            Action = ActionType.CreateArticle
        };

        // Assert
        Assert.Equal(userGroup.Guid, permission.UserGroupGuid);
    }

    [Fact]
    public void UserGroup_Should_ReturnAssignedUserGroup()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var permission = new UserGroupPermission
        {
            UserGroup = userGroup,
            Action = ActionType.CreateArticle
        };

        // Assert
        Assert.Same(userGroup, permission.UserGroup);
    }

    [Fact]
    public void Action_Should_ReturnAssignedAction()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var permission = new UserGroupPermission
        {
            UserGroup = userGroup,
            Action = ActionType.CreateArticle
        };

        // Assert
        Assert.Equal(ActionType.CreateArticle, permission.Action);
    }

    [Fact]
    public void ParentAuditedEntity_Should_ReturnUserGroup()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var permission = new UserGroupPermission
        {
            UserGroup = userGroup,
            Action = ActionType.CreateArticle
        };

        // Assert
        Assert.Same(userGroup, permission.ParentAuditedEntity);
    }

    [Fact]
    public void ParentAuditedEntity_Should_BeOfTypeUserGroup()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var permission = new UserGroupPermission
        {
            UserGroup = userGroup,
            Action = ActionType.CreateArticle
        };

        // Assert
        Assert.IsType<UserGroup>(permission.ParentAuditedEntity);
    }
}
