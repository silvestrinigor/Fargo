using Fargo.Core.Actors;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Fargo.Core.UserGroups;
using Fargo.Core.Users;

namespace Fargo.Core.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateAdminUser_WhenValid_ShouldBeAdminUser()
    {
        var admin = User.CreateAdministratorUser(default);

        Assert.Equal(FargoCoreWellKnowGuids.AdminUserGuid, admin.Guid);
        Assert.True(admin.IsAdmin);
    }

    [Fact]
    public void CreateNormalUser_WhenValid_ShouldNotBeAdminUser()
    {
        var user = User.CreateUser(default);

        Assert.NotEqual(FargoCoreWellKnowGuids.AdminUserGuid, user.Guid);
        Assert.False(user.IsAdmin);
    }

    [Fact]
    public void CreateUser_ShouldBeActiveAndInGlobalPartition()
    {
        var user = User.CreateUser(default);

        Assert.True(user.IsActive);
        Assert.Contains(FargoCoreWellKnowGuids.GlobalPartitionGuid, user.PartitionGuids);
    }

    [Fact]
    public void CreateAdministratorUser_ShouldBeInAdministratorsGroupAndHaveGlobalAccess()
    {
        var admin = User.CreateAdministratorUser(default);

        Assert.Contains(admin.UserGroupMemberships, m => m.UserGroupGuid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid);
        Assert.Contains(admin.PartitionAccesses, a => a.PartitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid);
    }

    [Fact]
    public void AddPermission_ShouldBeIdempotent()
    {
        var user = User.CreateUser(default);

        user.AddPermission(ActionType.CreateItem);
        user.AddPermission(ActionType.CreateItem);

        Assert.Single(user.Permissions, p => p == ActionType.CreateItem);
    }

    [Fact]
    public void RemovePermission_WhenNormalUser_ShouldRemove()
    {
        var user = User.CreateUser(default);
        user.AddPermission(ActionType.CreateItem);

        user.RemovePermission(ActionType.CreateItem);

        Assert.DoesNotContain(ActionType.CreateItem, user.Permissions);
    }

    [Fact]
    public void RemovePermission_WhenAdmin_ShouldThrow()
    {
        var admin = User.CreateAdministratorUser(default);
        admin.AddPermission(ActionType.CreateItem);

        var ex = Assert.Throws<FargoCoreException>(() => admin.RemovePermission(ActionType.CreateItem));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Deactivate_WhenNormalUser_ShouldSetInactive()
    {
        var user = User.CreateUser(default);

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAdmin_ShouldThrow()
    {
        var admin = User.CreateAdministratorUser(default);

        var ex = Assert.Throws<FargoCoreException>(() => admin.Deactivate());

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var user = User.CreateUser(default);
        user.Deactivate();

        user.Activate();

        Assert.True(user.IsActive);
    }

    [Fact]
    public void AddPartition_WhenAdminAndNonGlobal_ShouldThrow()
    {
        var admin = User.CreateAdministratorUser(default);
        var global = Partition.CreateGlobalPartition(default);
        var other = Partition.CreatePartition(new Name("Other"), global);

        var ex = Assert.Throws<FargoCoreException>(() => admin.AddPartition(other));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void RemovePartition_WhenGlobal_ShouldThrow()
    {
        var user = User.CreateUser(default);

        Assert.Throws<FargoCoreException>(
            () => user.RemovePartition(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void RemovePartitionAccess_WhenAdminAndGlobal_ShouldThrow()
    {
        var admin = User.CreateAdministratorUser(default);

        Assert.Throws<FargoCoreException>(
            () => admin.RemovePartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void RemoveUserGroup_WhenAdminAndAdministratorsGroup_ShouldThrow()
    {
        var admin = User.CreateAdministratorUser(default);

        Assert.Throws<FargoCoreException>(
            () => admin.RemoveUserGroup(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid));
    }

    [Fact]
    public void AddUserGroup_ShouldBeIdempotent()
    {
        var user = User.CreateUser(default);
        var group = UserGroup.CreateUserGroup(new Nameid("grp"));

        user.AddUserGroup(group);
        user.AddUserGroup(group);

        Assert.Single(user.UserGroupMemberships, m => m.UserGroupGuid == group.Guid);
    }
}

