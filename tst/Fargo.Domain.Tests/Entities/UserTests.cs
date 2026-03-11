using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void Description_Should_DefaultToEmpty_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Equal(Description.Empty, user.Description);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_DefaultToNinetyDays()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Equal(
            TimeSpan.FromDays(User.DefaultPasswordChangeDays),
            user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_SetValue_When_ValueIsPositive()
    {
        // Arrange
        var user = CreateUser();
        var value = TimeSpan.FromDays(30);

        // Act
        user.DefaultPasswordExpirationPeriod = value;

        // Assert
        Assert.Equal(value, user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_SetValue_When_ValueIsZero()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.DefaultPasswordExpirationPeriod = TimeSpan.Zero;

        // Assert
        Assert.Equal(TimeSpan.Zero, user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_ThrowArgumentOutOfRangeException_When_ValueIsNegative()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(-1);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void RequirePasswordChangeAt_Should_DefaultToFutureDate()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var user = CreateUser();
        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before.AddDays(User.DefaultPasswordChangeDays));
        Assert.True(user.RequirePasswordChangeAt <= after.AddDays(User.DefaultPasswordChangeDays));
    }

    [Fact]
    public void ResetPasswordExpiration_Should_SetExpirationBasedOnDefaultPasswordExpirationPeriod()
    {
        // Arrange
        var user = CreateUser();
        var expiration = TimeSpan.FromDays(15);
        user.DefaultPasswordExpirationPeriod = expiration;

        var before = DateTimeOffset.UtcNow;

        // Act
        user.ResetPasswordExpiration();

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before + expiration);
        Assert.True(user.RequirePasswordChangeAt <= after + expiration);
    }

    [Fact]
    public void ResetPasswordExpiration_Should_SetExpirationToNow_When_DefaultPasswordExpirationPeriodIsZero()
    {
        // Arrange
        var user = CreateUser();
        user.DefaultPasswordExpirationPeriod = TimeSpan.Zero;

        var before = DateTimeOffset.UtcNow;

        // Act
        user.ResetPasswordExpiration();

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void ResetPasswordExpiration_Should_OverwritePreviousExpirationDate()
    {
        // Arrange
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddDays(-10));

        user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(20);

        // Act
        user.ResetPasswordExpiration();

        // Assert
        Assert.True(user.RequirePasswordChangeAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_SetFutureExpirationDate_When_DaysIsValid()
    {
        // Arrange
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        // Act
        user.RequirePasswordChangeInDays(10);

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before.AddDays(10));
        Assert.True(user.RequirePasswordChangeAt <= after.AddDays(10));
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_SetExpirationToNow_When_DaysIsZero()
    {
        // Arrange
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        // Act
        user.RequirePasswordChangeInDays(0);

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_ThrowArgumentOutOfRangeException_When_DaysIsNegative()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.RequirePasswordChangeInDays(-1);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void MarkPasswordChangeAsRequired_Should_SetExpirationToNow()
    {
        // Arrange
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        // Act
        user.MarkPasswordChangeAsRequired();

        var after = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnTrue_When_ExpirationDateIsNow()
    {
        // Arrange
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow);

        // Assert
        Assert.True(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnTrue_When_ExpirationDateIsInThePast()
    {
        // Arrange
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        // Assert
        Assert.True(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnFalse_When_ExpirationDateIsInTheFuture()
    {
        // Arrange
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddMinutes(1));

        // Assert
        Assert.False(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void AddPermission_Should_AddPermission_When_ItDoesNotExist()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.AddPermission(ActionType.CreateUser);

        // Assert
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
    }

    [Fact]
    public void AddPermission_Should_NotAddDuplicatePermission_When_ItAlreadyExists()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateUser);

        // Act
        user.AddPermission(ActionType.CreateUser);

        // Assert
        Assert.Equal(1, user.UserPermissions.Count(x => x.Action == ActionType.CreateUser));
    }

    [Fact]
    public void AddPermission_Should_AssociatePermissionWithUser()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.AddPermission(ActionType.DeleteUser);

        // Assert
        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(ActionType.DeleteUser, permission.Action);
        Assert.Equal(user, permission.User);
        Assert.Equal(user.Guid, permission.UserGuid);
    }

    [Fact]
    public void RemovePermission_Should_RemovePermission_When_ItExists()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        // Act
        user.RemovePermission(ActionType.EditUser);

        // Assert
        Assert.DoesNotContain(user.UserPermissions, x => x.Action == ActionType.EditUser);
    }

    [Fact]
    public void RemovePermission_Should_DoNothing_When_PermissionDoesNotExist()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.CreateUser);

        // Act
        user.RemovePermission(ActionType.DeleteUser);

        // Assert
        Assert.Single(user.UserPermissions);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasDirectPermission()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        // Act
        var result = user.HasPermission(ActionType.EditUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_ActiveGroupHasPermission()
    {
        // Arrange
        var group = CreateUserGroup();
        group.AddPermission(ActionType.EditUser);

        var user = CreateUser();
        user.AddGroup(group);

        // Act
        var result = user.HasPermission(ActionType.EditUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_OnlyInactiveGroupHasPermission()
    {
        // Arrange
        var group = CreateUserGroup(isActive: false);
        group.AddPermission(ActionType.EditUser);

        var user = CreateUser();
        user.AddGroup(group);

        // Act
        var result = user.HasPermission(ActionType.EditUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasNoDirectPermission_But_AnotherActiveGroupHasIt()
    {
        // Arrange
        var groupWithoutPermission = CreateUserGroup();
        var groupWithPermission = CreateUserGroup();
        groupWithPermission.AddPermission(ActionType.DeleteUser);

        var user = CreateUser();
        user.AddGroup(groupWithoutPermission);
        user.AddGroup(groupWithPermission);

        // Act
        var result = user.HasPermission(ActionType.DeleteUser);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_UserDoesNotHavePermission()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var result = user.HasPermission(ActionType.EditUser);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_AfterPermissionIsRemoved()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        // Act
        user.RemovePermission(ActionType.EditUser);

        // Assert
        Assert.False(user.HasPermission(ActionType.EditUser));
    }

    [Fact]
    public void ValidatePermission_Should_NotThrow_When_UserHasPermission()
    {
        // Arrange
        var user = CreateUser();
        user.AddPermission(ActionType.DeleteUser);

        // Act
        user.ValidatePermission(ActionType.DeleteUser);

        // Assert
    }

    [Fact]
    public void ValidatePermission_Should_NotThrow_When_ActiveGroupGrantsPermission()
    {
        // Arrange
        var group = CreateUserGroup();
        group.AddPermission(ActionType.DeleteUser);

        var user = CreateUser();
        user.AddGroup(group);

        // Act
        user.ValidatePermission(ActionType.DeleteUser);

        // Assert
    }

    [Fact]
    public void ValidatePermission_Should_ThrowUserNotAuthorizedFargoDomainException_When_UserDoesNotHavePermission()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.ValidatePermission(ActionType.DeleteUser);

        // Assert
        var exception = Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
    }

    [Fact]
    public void ValidatePermission_Should_ThrowUserNotAuthorizedFargoDomainException_WithExpectedData()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.ValidatePermission(ActionType.DeleteUser);

        // Assert
        var exception = Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
        Assert.Equal(ActionType.DeleteUser, exception.ActionType);
    }

    [Fact]
    public void UserPermissions_Should_CopyInputCollection_When_Initialized()
    {
        // Arrange
        var owner = CreateUser();
        var anotherUser = CreateUser();

        var sourcePermissions = new List<UserPermission>
        {
            new()
            {
                User = owner,
                Action = ActionType.CreateUser
            }
        };

        var user = CreateUser(
            guid: anotherUser.Guid,
            userPermissions: sourcePermissions);

        // Act
        sourcePermissions.Add(new UserPermission
        {
            User = owner,
            Action = ActionType.DeleteUser
        });

        // Assert
        Assert.Single(user.UserPermissions);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
        Assert.DoesNotContain(user.UserPermissions, x => x.Action == ActionType.DeleteUser);
    }

    [Fact]
    public void UserGroups_Should_CopyInputCollection_When_Initialized()
    {
        // Arrange
        var sourceGroups = new List<UserGroup>
        {
            CreateUserGroup(nameid: "admins")
        };

        var user = CreateUser(userGroups: sourceGroups);

        // Act
        sourceGroups.Add(CreateUserGroup(nameid: "managers"));

        // Assert
        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Nameid == new Nameid("admins"));
        Assert.DoesNotContain(user.UserGroups, x => x.Nameid == new Nameid("managers"));
    }

    [Fact]
    public void UserPermission_Should_SynchronizeUserGuid_When_Initialized()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var permission = new UserPermission
        {
            User = user,
            Action = ActionType.EditUser
        };

        // Assert
        Assert.Equal(user.Guid, permission.UserGuid);
        Assert.Equal(user, permission.User);
    }

    [Fact]
    public void FirstName_Should_DefaultToNull_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Null(user.FirstName);
    }

    [Fact]
    public void LastName_Should_DefaultToNull_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Null(user.LastName);
    }

    [Fact]
    public void UserPermissions_Should_DefaultToEmpty_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Empty(user.UserPermissions);
    }

    [Fact]
    public void UserGroups_Should_DefaultToEmpty_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.Empty(user.UserGroups);
    }

    [Fact]
    public void IsActive_Should_DefaultToTrue_When_NotSpecified()
    {
        // Arrange
        var user = CreateUser();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void IsActive_Should_SetFalse_When_SpecifiedAsFalse()
    {
        // Arrange
        var user = CreateUser(isActive: false);

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void IsActive_Should_SetTrue_When_SpecifiedAsTrue()
    {
        // Arrange
        var user = CreateUser(isActive: true);

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Activate_Should_SetIsActive_ToTrue()
    {
        // Arrange
        var user = CreateUser(isActive: false);

        // Act
        user.Activate();

        // Assert
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActive_ToFalse()
    {
        // Arrange
        var user = CreateUser(isActive: true);

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }

    [Fact]
    public void ValidateIsActive_Should_NotThrow_When_UserIsActive()
    {
        // Arrange
        var user = CreateUser(isActive: true);

        // Act
        user.ValidateIsActive();

        // Assert
    }

    [Fact]
    public void ValidateIsActive_Should_ThrowUserInactiveFargoDomainException_When_UserIsInactive()
    {
        // Arrange
        var user = CreateUser(isActive: false);

        // Act
        void act() => user.ValidateIsActive();

        // Assert
        var exception = Assert.Throws<UserInactiveFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
    }

    [Fact]
    public void AddGroup_Should_AddGroup_When_ItDoesNotExist()
    {
        // Arrange
        var user = CreateUser();
        var group = CreateUserGroup();

        // Act
        user.AddGroup(group);

        // Assert
        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == group.Guid);
    }

    [Fact]
    public void AddGroup_Should_NotAddDuplicateGroup_When_GroupAlreadyExists()
    {
        // Arrange
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);

        // Act
        user.AddGroup(group);

        // Assert
        Assert.Single(user.UserGroups);
    }

    [Fact]
    public void AddGroup_Should_NotAddDuplicateGroup_When_GroupWithSameGuidAlreadyExists()
    {
        // Arrange
        var groupGuid = Guid.NewGuid();

        var first = CreateUserGroup(guid: groupGuid, nameid: "admins");
        var second = CreateUserGroup(guid: groupGuid, nameid: "managers");

        var user = CreateUser();
        user.AddGroup(first);

        // Act
        user.AddGroup(second);

        // Assert
        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == groupGuid);
    }

    [Fact]
    public void AddGroup_Should_ThrowArgumentNullException_When_GroupIsNull()
    {
        // Arrange
        var user = CreateUser();

        // Act
        void act() => user.AddGroup(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void RemoveGroup_Should_RemoveGroup_When_ItExists()
    {
        // Arrange
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);

        // Act
        user.RemoveGroup(group.Guid);

        // Assert
        Assert.Empty(user.UserGroups);
    }

    [Fact]
    public void RemoveGroup_Should_DoNothing_When_GroupDoesNotExist()
    {
        // Arrange
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);

        // Act
        user.RemoveGroup(Guid.NewGuid());

        // Assert
        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == group.Guid);
    }

    private static User CreateUser(
        Guid? guid = null,
        Description? description = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        DateTimeOffset? requirePasswordChangeAt = null,
        IReadOnlyCollection<UserPermission>? userPermissions = null,
        IReadOnlyCollection<UserGroup>? userGroups = null,
        bool isActive = true)
    {
        return new User
        {
            Guid = guid ?? Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            Description = description ?? Description.Empty,
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength)),
            DefaultPasswordExpirationPeriod =
                defaultPasswordExpirationPeriod ?? TimeSpan.FromDays(User.DefaultPasswordChangeDays),
            RequirePasswordChangeAt =
                requirePasswordChangeAt ?? DateTimeOffset.UtcNow.AddDays(User.DefaultPasswordChangeDays),
            UserPermissions = userPermissions ?? [],
            UserGroups = userGroups ?? [],
            IsActive = isActive
        };
    }

    private static UserGroup CreateUserGroup(
        Guid? guid = null,
        string nameid = "admins",
        bool isActive = true)
    {
        return new UserGroup
        {
            Guid = guid ?? Guid.NewGuid(),
            Nameid = new Nameid(nameid),
            IsActive = isActive
        };
    }
}