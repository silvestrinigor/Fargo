using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserPermissionTests
{
    [Fact]
    public void ObjectInitializer_Should_SetUser_Action_And_SynchronizeUserGuid()
    {
        // Arrange
        var userGuid = Guid.NewGuid();
        var user = CreateUser(userGuid);

        // Act
        var permission = CreateUserPermission(user, ActionType.CreateArticle);

        // Assert
        Assert.Equal(user, permission.User);
        Assert.Equal(ActionType.CreateArticle, permission.Action);
        Assert.Equal(userGuid, permission.UserGuid);
    }

    [Fact]
    public void UserGuid_Should_Match_AssignedUserGuid()
    {
        // Arrange
        var firstUser = CreateUser(Guid.NewGuid());
        var secondUser = CreateUser(Guid.NewGuid());

        // Act
        var firstPermission = CreateUserPermission(firstUser, ActionType.CreateArticle);
        var secondPermission = CreateUserPermission(secondUser, ActionType.CreateArticle);

        // Assert
        Assert.Equal(firstUser.Guid, firstPermission.UserGuid);
        Assert.Equal(secondUser.Guid, secondPermission.UserGuid);
        Assert.NotEqual(firstPermission.UserGuid, secondPermission.UserGuid);
    }

    private static UserPermission CreateUserPermission(User user, ActionType action)
    {
        return new UserPermission
        {
            User = user,
            Action = action
        };
    }

    private static User CreateUser(Guid guid)
    {
        var user = (User)RuntimeHelpers.GetUninitializedObject(typeof(User));

        var guidProperty = typeof(Entity).GetProperty(
            "Guid",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

        guidProperty.SetMethod!.Invoke(user, new object[] { guid });

        return user;
    }
}