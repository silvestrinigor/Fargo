using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;

namespace Fargo.Core.Tests.Users;

public class UserTests
{
    [Fact]
    public void CreateAdminUser_WhenValid_ShouldBeAdminUser()
    {
        var admin = Actor.NewAdministratorUser(default);

        Assert.Equal(FargoCoreWellKnowGuids.AdminUserGuid, admin.Guid);
        Assert.True(admin.User!.IsAdmin);
    }

    [Fact]
    public void CreateNormalUser_WhenValid_ShouldNotBeAdminUser()
    {
        var actor = Actor.NewUser(default);

        Assert.NotEqual(FargoCoreWellKnowGuids.AdminUserGuid, actor.Guid);
        Assert.False(actor.User!.IsAdmin);
    }

    [Fact]
    public void CreateUser_ShouldBeActiveAndInGlobalPartition()
    {
        var actor = Actor.NewUser(default);

        Assert.True(actor.IsActive);
        Assert.Contains(FargoCoreWellKnowGuids.GlobalPartitionGuid, actor.PartitionGuids);
    }

    [Fact]
    public void CreateAdministratorUser_ShouldBeInAdministratorsGroupAndHaveGlobalAccess()
    {
        var admin = Actor.NewAdministratorUser(default);

        Assert.Contains(admin.User!.UserGroupMemberships, m => m.UserGroupGuid == FargoCoreWellKnowGuids.AdministratorsUserGroupGuid);
        Assert.Contains(admin.PartitionAccesses, a => a.PartitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid);
    }

    [Fact]
    public void AddPermission_ShouldBeIdempotent()
    {
        var actor = Actor.NewUser(default);

        actor.AddPermission(ActionType.CreateItem);
        actor.AddPermission(ActionType.CreateItem);

        Assert.Single(actor.Permissions, p => p == ActionType.CreateItem);
    }

    [Fact]
    public void RemovePermission_WhenNormalUser_ShouldRemove()
    {
        var actor = Actor.NewUser(default);
        actor.AddPermission(ActionType.CreateItem);

        actor.RemovePermission(ActionType.CreateItem);

        Assert.DoesNotContain(ActionType.CreateItem, actor.Permissions);
    }

    [Fact]
    public void RemovePermission_WhenAdmin_ShouldThrow()
    {
        var admin = Actor.NewAdministratorUser(default);
        admin.AddPermission(ActionType.CreateItem);

        var ex = Assert.Throws<FargoCoreException>(() => admin.RemovePermission(ActionType.CreateItem));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Deactivate_WhenNormalUser_ShouldSetInactive()
    {
        var actor = Actor.NewUser(default);

        actor.Deactivate();

        Assert.False(actor.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAdmin_ShouldThrow()
    {
        var admin = Actor.NewAdministratorUser(default);

        var ex = Assert.Throws<FargoCoreException>(() => admin.Deactivate());

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var actor = Actor.NewUser(default);
        actor.Deactivate();

        actor.Activate();

        Assert.True(actor.IsActive);
    }

    [Fact]
    public void AddPartition_WhenAdminAndNonGlobal_ShouldThrow()
    {
        var admin = Actor.NewAdministratorUser(default);
        var global = Partition.CreateGlobalPartition(default);
        var other = Partition.CreatePartition(new Name("Other"), global);

        var ex = Assert.Throws<FargoCoreException>(() => admin.AddPartition(other));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void RemovePartition_WhenGlobal_ShouldThrow()
    {
        var actor = Actor.NewUser(default);

        Assert.Throws<FargoCoreException>(
            () => actor.RemovePartition(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void RemovePartitionAccess_WhenAdminAndGlobal_ShouldThrow()
    {
        var admin = Actor.NewAdministratorUser(default);

        Assert.Throws<FargoCoreException>(
            () => admin.RemovePartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void RemoveUserGroup_WhenAdminAndAdministratorsGroup_ShouldThrow()
    {
        var admin = Actor.NewAdministratorUser(default).User!;

        Assert.Throws<FargoCoreException>(
            () => admin.RemoveUserGroup(FargoCoreWellKnowGuids.AdministratorsUserGroupGuid));
    }

    [Fact]
    public void AddUserGroup_ShouldBeIdempotent()
    {
        var user = Actor.NewUser(default).User!;
        var group = UserGroup.CreateUserGroup(new Nameid("grp"));

        user.AddUserGroup(group);
        user.AddUserGroup(group);

        Assert.Single(user.UserGroupMemberships, m => m.UserGroupGuid == group.Guid);
    }
}
