using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Tests.Entities;

public sealed class UserGroupTests
{
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
    public void Description_Should_BeEmpty_ByDefault()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        var result = userGroup.Description;

        // Assert
        Assert.Equal(Description.Empty, result);
    }

    [Fact]
    public void UserGroupPermissions_Should_BeEmpty_ByDefault()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Assert
        Assert.Empty(userGroup.UserGroupPermissions);
    }

    [Fact]
    public void Partitions_Should_BeEmpty_ByDefault()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Assert
        Assert.Empty(userGroup.Partitions);
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
        Assert.Equal(userGroup.Guid, exception.UserGroupGuid);
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
        Assert.Equal(userGroup.Guid, permission.UserGroupGuid);
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
    public void AddPartition_Should_AddPartition_When_ItDoesNotExist()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var partition = CreatePartition();

        // Act
        userGroup.AddPartition(partition);

        // Assert
        Assert.Single(userGroup.Partitions);
        Assert.Contains(userGroup.Partitions, x => x.Guid == partition.Guid);
    }

    [Fact]
    public void AddPartition_Should_NotAddDuplicate_When_ItAlreadyExists()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var partition = CreatePartition();

        userGroup.AddPartition(partition);

        // Act
        userGroup.AddPartition(partition);

        // Assert
        Assert.Single(userGroup.Partitions);
    }

    [Fact]
    public void AddPartition_Should_NotAddDuplicate_When_PartitionWithSameGuidAlreadyExists()
    {
        // Arrange
        var partitionGuid = Guid.NewGuid();
        var first = CreatePartition(guid: partitionGuid, name: "Partition A");
        var second = CreatePartition(guid: partitionGuid, name: "Partition B");
        var userGroup = CreateUserGroup();

        userGroup.AddPartition(first);

        // Act
        userGroup.AddPartition(second);

        // Assert
        Assert.Single(userGroup.Partitions);
        Assert.Contains(userGroup.Partitions, x => x.Guid == partitionGuid);
    }

    [Fact]
    public void AddPartition_Should_ThrowArgumentNullException_When_PartitionIsNull()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        void act() => userGroup.AddPartition(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void RemovePartition_Should_RemovePartition_When_ItExists()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var partition = CreatePartition();

        userGroup.AddPartition(partition);

        // Act
        userGroup.RemovePartition(partition.Guid);

        // Assert
        Assert.Empty(userGroup.Partitions);
    }

    [Fact]
    public void RemovePartition_Should_DoNothing_When_ItDoesNotExist()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var partition = CreatePartition();

        userGroup.AddPartition(partition);

        // Act
        userGroup.RemovePartition(Guid.NewGuid());

        // Assert
        Assert.Single(userGroup.Partitions);
        Assert.Contains(userGroup.Partitions, x => x.Guid == partition.Guid);
    }

    [Fact]
    public void CanBeAccessedBy_Should_ReturnTrue_When_GroupHasNoPartitions()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var user = CreateUser();

        // Act
        var result = userGroup.CanBeAccessedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBeAccessedBy_Should_ReturnTrue_When_UserHasAccessToAtLeastOneGroupPartition()
    {
        // Arrange
        var accessiblePartition = CreatePartition(name: "Accessible");
        var otherPartition = CreatePartition(name: "Other");

        var userGroup = CreateUserGroup();
        userGroup.AddPartition(accessiblePartition);
        userGroup.AddPartition(otherPartition);

        var user = CreateUser();
        user.AddPartitionAccess(accessiblePartition);

        // Act
        var result = userGroup.CanBeAccessedBy(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanBeAccessedBy_Should_ReturnFalse_When_UserDoesNotHaveAccessToAnyGroupPartition()
    {
        // Arrange
        var groupPartition = CreatePartition();

        var userGroup = CreateUserGroup();
        userGroup.AddPartition(groupPartition);

        var user = CreateUser();

        // Act
        var result = userGroup.CanBeAccessedBy(user);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanBeAccessedBy_Should_ThrowArgumentNullException_When_UserIsNull()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        void act() => userGroup.CanBeAccessedBy(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ReturnTrue_When_GroupIsActive_HasPermission_And_UserCanAccessIt()
    {
        // Arrange
        var partition = CreatePartition();

        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);
        userGroup.AddPartition(partition);

        var user = CreateUser();
        user.AddPartitionAccess(partition);

        // Act
        var result = userGroup.GrantsPermissionTo(user, ActionType.CreateArticle);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ReturnTrue_When_GroupIsActive_HasPermission_And_GroupHasNoPartitions()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);

        var user = CreateUser();

        // Act
        var result = userGroup.GrantsPermissionTo(user, ActionType.CreateArticle);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ReturnFalse_When_GroupIsInactive()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);
        userGroup.Deactivate();

        var user = CreateUser();

        // Act
        var result = userGroup.GrantsPermissionTo(user, ActionType.CreateArticle);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ReturnFalse_When_GroupDoesNotHavePermission()
    {
        // Arrange
        var userGroup = CreateUserGroup();
        var user = CreateUser();

        // Act
        var result = userGroup.GrantsPermissionTo(user, ActionType.CreateArticle);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ReturnFalse_When_UserCannotAccessGroupPartitions()
    {
        // Arrange
        var partition = CreatePartition();

        var userGroup = CreateUserGroup();
        userGroup.AddPermission(ActionType.CreateArticle);
        userGroup.AddPartition(partition);

        var user = CreateUser();

        // Act
        var result = userGroup.GrantsPermissionTo(user, ActionType.CreateArticle);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GrantsPermissionTo_Should_ThrowArgumentNullException_When_UserIsNull()
    {
        // Arrange
        var userGroup = CreateUserGroup();

        // Act
        void act() => userGroup.GrantsPermissionTo(null!, ActionType.CreateArticle);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void UserGroupPermissions_Should_CopyInputCollection_When_Initialized()
    {
        // Arrange
        var owner = CreateUserGroup();

        var sourcePermissions = new List<UserGroupPermission>
        {
            new()
            {
                UserGroup = owner,
                Action = ActionType.CreateArticle
            }
        };

        var userGroup = CreateUserGroup(userGroupPermissions: sourcePermissions);

        // Act
        sourcePermissions.Add(new UserGroupPermission
        {
            UserGroup = owner,
            Action = ActionType.DeleteArticle
        });

        // Assert
        Assert.Single(userGroup.UserGroupPermissions);
        Assert.Contains(userGroup.UserGroupPermissions, x => x.Action == ActionType.CreateArticle);
        Assert.DoesNotContain(userGroup.UserGroupPermissions, x => x.Action == ActionType.DeleteArticle);
    }

    [Fact]
    public void Partitions_Should_CopyInputCollection_When_Initialized()
    {
        // Arrange
        var sourcePartitions = new List<Partition>
        {
            CreatePartition(name: "Partition A")
        };

        var userGroup = CreateUserGroup(partitions: sourcePartitions);

        // Act
        sourcePartitions.Add(CreatePartition(name: "Partition B"));

        // Assert
        Assert.Single(userGroup.Partitions);
        Assert.Contains(userGroup.Partitions, x => x.Name == new Name("Partition A"));
        Assert.DoesNotContain(userGroup.Partitions, x => x.Name == new Name("Partition B"));
    }

    private static UserGroup CreateUserGroup(
        Guid? guid = null,
        string nameid = "admins",
        bool isActive = true,
        IReadOnlyCollection<UserGroupPermission>? userGroupPermissions = null,
        IReadOnlyCollection<Partition>? partitions = null)
    {
        return new UserGroup
        {
            Guid = guid ?? Guid.NewGuid(),
            Nameid = new Nameid(nameid),
            IsActive = isActive,
            UserGroupPermissions = userGroupPermissions ?? [],
            Partitions = partitions ?? []
        };
    }

    private static User CreateUser(
        Guid? guid = null,
        IReadOnlyCollection<PartitionAccess>? partitionsAccesses = null)
    {
        return new User
        {
            Guid = guid ?? Guid.NewGuid(),
            Nameid = new Nameid("user123"),
            PasswordHash = new PasswordHash(new string('a', PasswordHash.MinLength)),
            PartitionsAccesses = partitionsAccesses ?? []
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
            IsActive = true
        };
    }
}