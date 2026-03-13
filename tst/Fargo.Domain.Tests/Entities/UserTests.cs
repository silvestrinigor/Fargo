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
        var user = CreateUser();
        Assert.Equal(Description.Empty, user.Description);
    }

    [Fact]
    public void FirstName_Should_DefaultToNull_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Null(user.FirstName);
    }

    [Fact]
    public void LastName_Should_DefaultToNull_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Null(user.LastName);
    }

    [Fact]
    public void UserPermissions_Should_DefaultToEmpty_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Empty(user.UserPermissions);
    }

    [Fact]
    public void UserGroups_Should_DefaultToEmpty_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Empty(user.UserGroups);
    }

    [Fact]
    public void PartitionsAccesses_Should_DefaultToEmpty_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Empty(user.PartitionsAccesses);
    }

    [Fact]
    public void Partitions_Should_DefaultToEmpty_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.Empty(user.Partitions);
    }

    [Fact]
    public void IsActive_Should_DefaultToTrue_When_NotSpecified()
    {
        var user = CreateUser();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void IsActive_Should_SetFalse_When_SpecifiedAsFalse()
    {
        var user = CreateUser(isActive: false);
        Assert.False(user.IsActive);
    }

    [Fact]
    public void IsActive_Should_SetTrue_When_SpecifiedAsTrue()
    {
        var user = CreateUser(isActive: true);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Activate_Should_SetIsActive_ToTrue()
    {
        var user = CreateUser(isActive: false);
        user.Activate();
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_Should_SetIsActive_ToFalse()
    {
        var user = CreateUser(isActive: true);
        user.Deactivate();
        Assert.False(user.IsActive);
    }

    [Fact]
    public void ValidateIsActive_Should_NotThrow_When_UserIsActive()
    {
        var user = CreateUser(isActive: true);
        user.ValidateIsActive();
    }

    [Fact]
    public void ValidateIsActive_Should_ThrowUserInactiveFargoDomainException_When_UserIsInactive()
    {
        var user = CreateUser(isActive: false);

        void act() => user.ValidateIsActive();

        var exception = Assert.Throws<UserInactiveFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_DefaultToNinetyDays()
    {
        var user = CreateUser();

        Assert.Equal(
            TimeSpan.FromDays(User.DefaultPasswordChangeDays),
            user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_SetValue_When_ValueIsPositive()
    {
        var user = CreateUser();
        var value = TimeSpan.FromDays(30);

        user.DefaultPasswordExpirationPeriod = value;

        Assert.Equal(value, user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_SetValue_When_ValueIsZero()
    {
        var user = CreateUser();

        user.DefaultPasswordExpirationPeriod = TimeSpan.Zero;

        Assert.Equal(TimeSpan.Zero, user.DefaultPasswordExpirationPeriod);
    }

    [Fact]
    public void DefaultPasswordExpirationPeriod_Should_ThrowArgumentOutOfRangeException_When_ValueIsNegative()
    {
        var user = CreateUser();

        void act() => user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(-1);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void RequirePasswordChangeAt_Should_DefaultToFutureDate()
    {
        var before = DateTimeOffset.UtcNow;
        var user = CreateUser();
        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before.AddDays(User.DefaultPasswordChangeDays));
        Assert.True(user.RequirePasswordChangeAt <= after.AddDays(User.DefaultPasswordChangeDays));
    }

    [Fact]
    public void ResetPasswordExpiration_Should_SetExpirationBasedOnDefaultPasswordExpirationPeriod()
    {
        var user = CreateUser();
        var expiration = TimeSpan.FromDays(15);
        user.DefaultPasswordExpirationPeriod = expiration;

        var before = DateTimeOffset.UtcNow;

        user.ResetPasswordExpiration();

        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before + expiration);
        Assert.True(user.RequirePasswordChangeAt <= after + expiration);
    }

    [Fact]
    public void ResetPasswordExpiration_Should_SetExpirationToNow_When_DefaultPasswordExpirationPeriodIsZero()
    {
        var user = CreateUser();
        user.DefaultPasswordExpirationPeriod = TimeSpan.Zero;

        var before = DateTimeOffset.UtcNow;

        user.ResetPasswordExpiration();

        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void ResetPasswordExpiration_Should_OverwritePreviousExpirationDate()
    {
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddDays(-10));

        user.DefaultPasswordExpirationPeriod = TimeSpan.FromDays(20);

        user.ResetPasswordExpiration();

        Assert.True(user.RequirePasswordChangeAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_SetFutureExpirationDate_When_DaysIsValid()
    {
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        user.RequirePasswordChangeInDays(10);

        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before.AddDays(10));
        Assert.True(user.RequirePasswordChangeAt <= after.AddDays(10));
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_SetExpirationToNow_When_DaysIsZero()
    {
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        user.RequirePasswordChangeInDays(0);

        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void RequirePasswordChangeInDays_Should_ThrowArgumentOutOfRangeException_When_DaysIsNegative()
    {
        var user = CreateUser();

        void act() => user.RequirePasswordChangeInDays(-1);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void MarkPasswordChangeAsRequired_Should_SetExpirationToNow()
    {
        var user = CreateUser();
        var before = DateTimeOffset.UtcNow;

        user.MarkPasswordChangeAsRequired();

        var after = DateTimeOffset.UtcNow;

        Assert.True(user.RequirePasswordChangeAt >= before);
        Assert.True(user.RequirePasswordChangeAt <= after);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnTrue_When_ExpirationDateIsNow()
    {
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow);

        Assert.True(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnTrue_When_ExpirationDateIsInThePast()
    {
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void IsPasswordChangeRequired_Should_ReturnFalse_When_ExpirationDateIsInTheFuture()
    {
        var user = CreateUser(
            requirePasswordChangeAt: DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.False(user.IsPasswordChangeRequired);
    }

    [Fact]
    public void AddPermission_Should_AddPermission_When_ItDoesNotExist()
    {
        var user = CreateUser();

        user.AddPermission(ActionType.CreateUser);

        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
    }

    [Fact]
    public void AddPermission_Should_NotAddDuplicatePermission_When_ItAlreadyExists()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.CreateUser);

        user.AddPermission(ActionType.CreateUser);

        Assert.Equal(1, user.UserPermissions.Count(x => x.Action == ActionType.CreateUser));
    }

    [Fact]
    public void AddPermission_Should_AssociatePermissionWithUser()
    {
        var user = CreateUser();

        user.AddPermission(ActionType.DeleteUser);

        var permission = Assert.Single(user.UserPermissions);
        Assert.Equal(ActionType.DeleteUser, permission.Action);
        Assert.Equal(user, permission.User);
        Assert.Equal(user.Guid, permission.UserGuid);
    }

    [Fact]
    public void RemovePermission_Should_RemovePermission_When_ItExists()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        user.RemovePermission(ActionType.EditUser);

        Assert.DoesNotContain(user.UserPermissions, x => x.Action == ActionType.EditUser);
    }

    [Fact]
    public void RemovePermission_Should_DoNothing_When_PermissionDoesNotExist()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.CreateUser);

        user.RemovePermission(ActionType.DeleteUser);

        Assert.Single(user.UserPermissions);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasDirectPermission()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        var result = user.HasPermission(ActionType.EditUser);

        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_ActiveGroupHasPermission_And_GroupHasNoPartitions()
    {
        var group = CreateUserGroup();
        group.AddPermission(ActionType.EditUser);

        var user = CreateUser();
        user.AddGroup(group);

        var result = user.HasPermission(ActionType.EditUser);

        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_ActiveGroupHasPermission_And_UserHasAccessToGroupPartition()
    {
        var partition = CreatePartition();

        var group = CreateUserGroup();
        group.AddPermission(ActionType.EditUser);
        group.AddPartition(partition);

        var user = CreateUser();
        user.AddPartitionAccess(partition);
        user.AddGroup(group);

        var result = user.HasPermission(ActionType.EditUser);

        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_ActiveGroupHasPermission_But_UserDoesNotHaveAccessToGroupPartition()
    {
        var groupPartition = CreatePartition();

        var group = CreateUserGroup();
        group.AddPermission(ActionType.EditUser);
        group.AddPartition(groupPartition);

        var user = CreateUser();
        user.AddGroup(group);

        var result = user.HasPermission(ActionType.EditUser);

        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_OnlyInactiveGroupHasPermission()
    {
        var group = CreateUserGroup(isActive: false);
        group.AddPermission(ActionType.EditUser);

        var user = CreateUser();
        user.AddGroup(group);

        var result = user.HasPermission(ActionType.EditUser);

        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnTrue_When_UserHasNoDirectPermission_But_AnotherActiveAccessibleGroupHasIt()
    {
        var inaccessiblePartition = CreatePartition(name: "Inaccessible");
        var accessiblePartition = CreatePartition(name: "Accessible");

        var groupWithoutPermission = CreateUserGroup();

        var inaccessibleGroupWithPermission = CreateUserGroup(nameid: "managers");
        inaccessibleGroupWithPermission.AddPermission(ActionType.DeleteUser);
        inaccessibleGroupWithPermission.AddPartition(inaccessiblePartition);

        var accessibleGroupWithPermission = CreateUserGroup(nameid: "supervisors");
        accessibleGroupWithPermission.AddPermission(ActionType.DeleteUser);
        accessibleGroupWithPermission.AddPartition(accessiblePartition);

        var user = CreateUser();
        user.AddPartitionAccess(accessiblePartition);
        user.AddGroup(groupWithoutPermission);
        user.AddGroup(inaccessibleGroupWithPermission);
        user.AddGroup(accessibleGroupWithPermission);

        var result = user.HasPermission(ActionType.DeleteUser);

        Assert.True(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_When_UserDoesNotHavePermission()
    {
        var user = CreateUser();

        var result = user.HasPermission(ActionType.EditUser);

        Assert.False(result);
    }

    [Fact]
    public void HasPermission_Should_ReturnFalse_AfterPermissionIsRemoved()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.EditUser);

        user.RemovePermission(ActionType.EditUser);

        Assert.False(user.HasPermission(ActionType.EditUser));
    }

    [Fact]
    public void ValidatePermission_Should_NotThrow_When_UserHasPermission()
    {
        var user = CreateUser();
        user.AddPermission(ActionType.DeleteUser);

        user.ValidatePermission(ActionType.DeleteUser);
    }

    [Fact]
    public void ValidatePermission_Should_NotThrow_When_ActiveAccessibleGroupGrantsPermission()
    {
        var partition = CreatePartition();

        var group = CreateUserGroup();
        group.AddPermission(ActionType.DeleteUser);
        group.AddPartition(partition);

        var user = CreateUser();
        user.AddPartitionAccess(partition);
        user.AddGroup(group);

        user.ValidatePermission(ActionType.DeleteUser);
    }

    [Fact]
    public void ValidatePermission_Should_ThrowUserNotAuthorizedFargoDomainException_When_GroupHasPermission_But_UserCannotAccessGroupPartitions()
    {
        var partition = CreatePartition();

        var group = CreateUserGroup();
        group.AddPermission(ActionType.DeleteUser);
        group.AddPartition(partition);

        var user = CreateUser();
        user.AddGroup(group);

        void act() => user.ValidatePermission(ActionType.DeleteUser);

        var exception = Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
        Assert.Equal(ActionType.DeleteUser, exception.ActionType);
    }

    [Fact]
    public void ValidatePermission_Should_ThrowUserNotAuthorizedFargoDomainException_When_UserDoesNotHavePermission()
    {
        var user = CreateUser();

        void act() => user.ValidatePermission(ActionType.DeleteUser);

        var exception = Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
    }

    [Fact]
    public void ValidatePermission_Should_ThrowUserNotAuthorizedFargoDomainException_WithExpectedData()
    {
        var user = CreateUser();

        void act() => user.ValidatePermission(ActionType.DeleteUser);

        var exception = Assert.Throws<UserNotAuthorizedFargoDomainException>(act);
        Assert.Equal(user.Guid, exception.UserGuid);
        Assert.Equal(ActionType.DeleteUser, exception.ActionType);
    }

    [Fact]
    public void AddGroup_Should_AddGroup_When_ItDoesNotExist()
    {
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);

        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == group.Guid);
    }

    [Fact]
    public void AddGroup_Should_NotAddDuplicateGroup_When_GroupAlreadyExists()
    {
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);
        user.AddGroup(group);

        Assert.Single(user.UserGroups);
    }

    [Fact]
    public void AddGroup_Should_NotAddDuplicateGroup_When_GroupWithSameGuidAlreadyExists()
    {
        var groupGuid = Guid.NewGuid();

        var first = CreateUserGroup(guid: groupGuid, nameid: "admins");
        var second = CreateUserGroup(guid: groupGuid, nameid: "managers");

        var user = CreateUser();
        user.AddGroup(first);
        user.AddGroup(second);

        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == groupGuid);
    }

    [Fact]
    public void AddGroup_Should_ThrowArgumentNullException_When_GroupIsNull()
    {
        var user = CreateUser();

        void act() => user.AddGroup(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void RemoveGroup_Should_RemoveGroup_When_ItExists()
    {
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);
        user.RemoveGroup(group.Guid);

        Assert.Empty(user.UserGroups);
    }

    [Fact]
    public void RemoveGroup_Should_DoNothing_When_GroupDoesNotExist()
    {
        var user = CreateUser();
        var group = CreateUserGroup();

        user.AddGroup(group);
        user.RemoveGroup(Guid.NewGuid());

        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Guid == group.Guid);
    }

    [Fact]
    public void AddPartitionAccess_Should_AddPartitionAccess_When_ItDoesNotExist()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartitionAccess(partition);

        var access = Assert.Single(user.PartitionsAccesses);
        Assert.Equal(user, access.User);
        Assert.Equal(user.Guid, access.UserGuid);
        Assert.Equal(partition, access.Partition);
        Assert.Equal(partition.Guid, access.PartitionGuid);
    }

    [Fact]
    public void AddPartitionAccess_Should_NotAddDuplicate_When_ItAlreadyExists()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartitionAccess(partition);
        user.AddPartitionAccess(partition);

        Assert.Single(user.PartitionsAccesses);
    }

    [Fact]
    public void AddPartitionAccess_Should_ThrowArgumentNullException_When_PartitionIsNull()
    {
        var user = CreateUser();

        void act() => user.AddPartitionAccess(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void RemovePartitionAccess_Should_RemovePartitionAccess_When_ItExists()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartitionAccess(partition);
        user.RemovePartitionAccess(partition.Guid);

        Assert.Empty(user.PartitionsAccesses);
    }

    [Fact]
    public void RemovePartitionAccess_Should_DoNothing_When_ItDoesNotExist()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartitionAccess(partition);
        user.RemovePartitionAccess(Guid.NewGuid());

        Assert.Single(user.PartitionsAccesses);
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnTrue_When_UserHasAccessToPartition()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartitionAccess(partition);

        var result = user.HasPartitionAccess(partition);

        Assert.True(result);
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnTrue_When_UserHasAccessToDifferentInstanceWithSameGuid()
    {
        var partitionGuid = Guid.NewGuid();

        var grantedPartition = CreatePartition(guid: partitionGuid, name: "Granted");
        var queriedPartition = CreatePartition(guid: partitionGuid, name: "Queried");

        var user = CreateUser();
        user.AddPartitionAccess(grantedPartition);

        var result = user.HasPartitionAccess(queriedPartition);

        Assert.True(result);
    }

    [Fact]
    public void HasPartitionAccess_Should_ReturnFalse_When_UserDoesNotHaveAccessToPartition()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        var result = user.HasPartitionAccess(partition);

        Assert.False(result);
    }

    [Fact]
    public void HasPartitionAccess_Should_ThrowArgumentNullException_When_PartitionIsNull()
    {
        var user = CreateUser();

        void act() => user.HasPartitionAccess(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void AddPartition_Should_AddPartition_When_ItDoesNotExist()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartition(partition);

        Assert.Single(user.Partitions);
        Assert.Contains(user.Partitions, x => x.Guid == partition.Guid);
    }

    [Fact]
    public void AddPartition_Should_NotAddDuplicate_When_ItAlreadyExists()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartition(partition);
        user.AddPartition(partition);

        Assert.Single(user.Partitions);
    }

    [Fact]
    public void AddPartition_Should_ThrowArgumentNullException_When_PartitionIsNull()
    {
        var user = CreateUser();

        void act() => user.AddPartition(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void RemovePartition_Should_RemovePartition_When_ItExists()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartition(partition);
        user.RemovePartition(partition.Guid);

        Assert.Empty(user.Partitions);
    }

    [Fact]
    public void RemovePartition_Should_DoNothing_When_ItDoesNotExist()
    {
        var user = CreateUser();
        var partition = CreatePartition();

        user.AddPartition(partition);
        user.RemovePartition(Guid.NewGuid());

        Assert.Single(user.Partitions);
    }

    [Fact]
    public void HasAccess_Should_ReturnTrue_When_EntityHasNoPartitions()
    {
        var user = CreateUser();
        var entity = CreatePartitionedEntity();

        var result = user.HasAccess(entity);

        Assert.True(result);
    }

    [Fact]
    public void HasAccess_Should_ReturnTrue_When_EntitySharesAtLeastOneAccessiblePartition()
    {
        var user = CreateUser();
        var accessiblePartition = CreatePartition();
        var otherPartition = CreatePartition();

        user.AddPartitionAccess(accessiblePartition);

        var entity = CreatePartitionedEntity([accessiblePartition, otherPartition]);

        var result = user.HasAccess(entity);

        Assert.True(result);
    }

    [Fact]
    public void HasAccess_Should_ReturnFalse_When_EntityDoesNotShareAnyAccessiblePartition()
    {
        var user = CreateUser();
        var userPartition = CreatePartition();
        var entityPartition = CreatePartition();

        user.AddPartitionAccess(userPartition);

        var entity = CreatePartitionedEntity([entityPartition]);

        var result = user.HasAccess(entity);

        Assert.False(result);
    }

    [Fact]
    public void HasAccess_Should_ThrowArgumentNullException_When_PartitionedIsNull()
    {
        var user = CreateUser();

        void act() => user.HasAccess(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void UserPermissions_Should_CopyInputCollection_When_Initialized()
    {
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

        sourcePermissions.Add(new UserPermission
        {
            User = owner,
            Action = ActionType.DeleteUser
        });

        Assert.Single(user.UserPermissions);
        Assert.Contains(user.UserPermissions, x => x.Action == ActionType.CreateUser);
        Assert.DoesNotContain(user.UserPermissions, x => x.Action == ActionType.DeleteUser);
    }

    [Fact]
    public void UserGroups_Should_CopyInputCollection_When_Initialized()
    {
        var sourceGroups = new List<UserGroup>
        {
            CreateUserGroup(nameid: "admins")
        };

        var user = CreateUser(userGroups: sourceGroups);

        sourceGroups.Add(CreateUserGroup(nameid: "managers"));

        Assert.Single(user.UserGroups);
        Assert.Contains(user.UserGroups, x => x.Nameid == new Nameid("admins"));
        Assert.DoesNotContain(user.UserGroups, x => x.Nameid == new Nameid("managers"));
    }

    [Fact]
    public void PartitionsAccesses_Should_CopyInputCollection_When_Initialized()
    {
        var owner = CreateUser();
        var partition = CreatePartition();

        var sourceAccesses = new List<PartitionAccess>
        {
            new()
            {
                User = owner,
                Partition = partition
            }
        };

        var user = CreateUser(partitionsAccesses: sourceAccesses);

        sourceAccesses.Add(new PartitionAccess
        {
            User = owner,
            Partition = CreatePartition()
        });

        Assert.Single(user.PartitionsAccesses);
    }

    [Fact]
    public void Partitions_Should_CopyInputCollection_When_Initialized()
    {
        var sourcePartitions = new List<Partition>
        {
            CreatePartition(name: "Partition A")
        };

        var user = CreateUser(partitions: sourcePartitions);

        sourcePartitions.Add(CreatePartition(name: "Partition B"));

        Assert.Single(user.Partitions);
        Assert.Contains(user.Partitions, x => x.Name == new Name("Partition A"));
        Assert.DoesNotContain(user.Partitions, x => x.Name == new Name("Partition B"));
    }

    [Fact]
    public void UserPermission_Should_SynchronizeUserGuid_When_Initialized()
    {
        var user = CreateUser();

        var permission = new UserPermission
        {
            User = user,
            Action = ActionType.EditUser
        };

        Assert.Equal(user.Guid, permission.UserGuid);
        Assert.Equal(user, permission.User);
    }

    private static User CreateUser(
        Guid? guid = null,
        Description? description = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        DateTimeOffset? requirePasswordChangeAt = null,
        IReadOnlyCollection<UserPermission>? userPermissions = null,
        IReadOnlyCollection<UserGroup>? userGroups = null,
        IReadOnlyCollection<PartitionAccess>? partitionsAccesses = null,
        IReadOnlyCollection<Partition>? partitions = null,
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
            PartitionsAccesses = partitionsAccesses ?? [],
            Partitions = partitions ?? [],
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

    private static Partition CreatePartition(
        Guid? guid = null,
        string name = "Partition A")
    {
        return new Partition
        {
            Guid = guid ?? Guid.NewGuid(),
            Name = new Name(name),
            Description = new Description("Partition description"),
            IsActive = true,
            IsEditable = true,
            IsGlobal = false
        };
    }

    private static TestPartitionedEntity CreatePartitionedEntity(
        IReadOnlyCollection<Partition>? partitions = null)
    {
        return new TestPartitionedEntity
        {
            Partitions = partitions ?? []
        };
    }

    private sealed class TestPartitionedEntity : IPartitioned
    {
        public IReadOnlyCollection<Partition> Partitions { get; init; } = [];
    }
}