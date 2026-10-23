using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;

namespace Fargo.Core.Tests.UserGroups;

public class UserGroupTests
{
    [Fact]
    public void CreateAdministratorsUserGroup_WhenValid_ShouldBeAdministrators()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);

        Assert.Equal(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid, administrators.Guid);
        Assert.True(administrators.IsAdministrators);
    }

    [Fact]
    public void CreateNormalUserGroup_WhenValid_ShouldNotBeAdministrators()
    {
        var userGroup = UserGroup.CreateUserGroup(default);

        Assert.NotEqual(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid, userGroup.Guid);
        Assert.False(userGroup.IsAdministrators);
    }

    [Fact]
    public void CreateAdministratorsUserGroup_ShouldHaveGlobalPartitionAccess()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);

        Assert.Contains(administrators.PartitionAccesses, a => a.PartitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid);
    }

    [Fact]
    public void AddPermission_ShouldBeIdempotent()
    {
        var group = UserGroup.CreateUserGroup(default);

        group.AddPermission(ActionType.CreateItem);
        group.AddPermission(ActionType.CreateItem);

        Assert.Single(group.Permissions, p => p == ActionType.CreateItem);
    }

    [Fact]
    public void RemovePermission_WhenAdministrators_ShouldThrow()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);
        administrators.AddPermission(ActionType.CreateItem);

        var ex = Assert.Throws<FargoCoreException>(() => administrators.RemovePermission(ActionType.CreateItem));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Deactivate_WhenAdministrators_ShouldThrow()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);

        var ex = Assert.Throws<FargoCoreException>(() => administrators.Deactivate());

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Deactivate_WhenNormalGroup_ShouldSetInactive()
    {
        var group = UserGroup.CreateUserGroup(default);

        group.Deactivate();

        Assert.False(group.IsActive);
    }

    [Fact]
    public void AddPartition_WhenAdministratorsAndNonGlobal_ShouldThrow()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);
        var global = Partition.CreateGlobalPartition(default);
        var other = Partition.CreatePartition(new Name("Other"), global);

        var ex = Assert.Throws<FargoCoreException>(() => administrators.AddPartition(other));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void RemovePartition_WhenGlobal_ShouldThrow()
    {
        var group = UserGroup.CreateUserGroup(default);

        Assert.Throws<FargoCoreException>(
            () => group.RemovePartition(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void RemovePartitionAccess_WhenAdministratorsAndGlobal_ShouldThrow()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(default);

        Assert.Throws<FargoCoreException>(
            () => administrators.RemovePartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void SetParentUserGroup_WhenSelf_ShouldThrow()
    {
        var group = UserGroup.CreateUserGroup(new Nameid("grp"));

        var ex = Assert.Throws<FargoCoreException>(() => group.SetParentUserGroup(group));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void SetParentUserGroup_WhenAdministrators_ShouldThrow()
    {
        var administrators = UserGroup.CreateAdministratorsUserGroup(new Nameid("admins"));
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));

        var ex = Assert.Throws<FargoCoreException>(() => administrators.SetParentUserGroup(parent));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void SetParentUserGroup_WhenValid_ShouldSetParent()
    {
        var child = UserGroup.CreateUserGroup(new Nameid("child"));
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));

        child.SetParentUserGroup(parent);

        Assert.Equal(parent.Guid, child.ParentUserGroupGuid);

        child.RemoveFromParentUserGroup();

        Assert.Null(child.ParentUserGroupGuid);
    }
}
