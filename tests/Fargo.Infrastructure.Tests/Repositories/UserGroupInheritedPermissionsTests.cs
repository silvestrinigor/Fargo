using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Repositories;

/// <summary>
/// The inherited-permissions query is a recursive CTE with PostgreSQL-specific JSON functions, so it
/// runs against a real PostgreSQL database instead of the in-memory provider.
/// </summary>
public class UserGroupInheritedPermissionsTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    // The database is shared by the tests of the class, and nameids are unique.
    private static Nameid UniqueNameid(string prefix)
        => new($"{prefix}-{Guid.NewGuid():N}"[..20]);

    private static UserGroup NewGroup(string prefix, params ActionType[] permissions)
    {
        var group = UserGroup.CreateUserGroup(UniqueNameid(prefix));

        foreach (var permission in permissions)
        {
            group.AddPermission(permission);
        }

        return group;
    }

    private async Task SaveAsync(params UserGroup[] groups)
    {
        await using var context = postgres.CreateContext();

        // Every user group belongs to the global partition, which the schema does not seed.
        if (!await context.Partitions.AnyAsync(p => p.Guid == FargoCoreWellKnowGuids.GlobalPartitionGuid))
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("global")));
        }

        context.UserGroups.AddRange(groups);

        await context.SaveChangesAsync();
    }

    private async Task<IReadOnlyCollection<ActionType>> InheritedAsync(Guid userGroupGuid)
    {
        await using var context = postgres.CreateContext();

        return await new UserGroupRepository(context).GetInheritedPermissionsAsync(userGroupGuid);
    }

    [Fact]
    public async Task GetInheritedPermissions_ShouldIncludeTheGroupAndEveryAncestor()
    {
        var grandparent = NewGroup("grandparent", ActionType.DeleteItem, ActionType.ReadAuditLogs);
        var parent = NewGroup("parent", ActionType.CreateItem);
        var child = NewGroup("child", ActionType.EditItem);
        parent.SetParentUserGroup(grandparent);
        child.SetParentUserGroup(parent);
        await SaveAsync(grandparent, parent, child);

        var permissions = await InheritedAsync(child.Guid);

        Assert.Equal(
            [ActionType.CreateItem, ActionType.DeleteItem, ActionType.EditItem, ActionType.ReadAuditLogs],
            permissions);
    }

    [Fact]
    public async Task GetInheritedPermissions_ShouldReturnEachPermissionOnce()
    {
        var parent = NewGroup("parent", ActionType.CreateItem, ActionType.DeleteItem);
        var child = NewGroup("child", ActionType.CreateItem);
        child.SetParentUserGroup(parent);
        await SaveAsync(parent, child);

        var permissions = await InheritedAsync(child.Guid);

        Assert.Equal([ActionType.CreateItem, ActionType.DeleteItem], permissions);
    }

    [Fact]
    public async Task GetInheritedPermissions_ShouldIncludeInactiveGroups()
    {
        var parent = NewGroup("parent", ActionType.DeleteItem);
        parent.Deactivate();
        var child = NewGroup("child", ActionType.CreateItem);
        child.SetParentUserGroup(parent);
        await SaveAsync(parent, child);

        var permissions = await InheritedAsync(child.Guid);

        Assert.Equal([ActionType.CreateItem, ActionType.DeleteItem], permissions);
    }

    [Fact]
    public async Task GetInheritedPermissions_ShouldNotIncludeDescendantsOrSiblings()
    {
        var parent = NewGroup("parent", ActionType.CreateItem);
        var group = NewGroup("group", ActionType.EditItem);
        var sibling = NewGroup("sibling", ActionType.DeleteItem);
        var descendant = NewGroup("descendant", ActionType.ReadAuditLogs);
        group.SetParentUserGroup(parent);
        sibling.SetParentUserGroup(parent);
        descendant.SetParentUserGroup(group);
        await SaveAsync(parent, group, sibling, descendant);

        var permissions = await InheritedAsync(group.Guid);

        Assert.Equal([ActionType.CreateItem, ActionType.EditItem], permissions);
    }

    [Fact]
    public async Task GetInheritedPermissions_WhenTheGroupHasNoParentAndNoPermissions_ShouldBeEmpty()
    {
        var group = NewGroup("empty");
        await SaveAsync(group);

        Assert.Empty(await InheritedAsync(group.Guid));
    }

    [Fact]
    public async Task GetInheritedPermissions_WhenTheGroupDoesNotExist_ShouldBeEmpty()
    {
        Assert.Empty(await InheritedAsync(Guid.NewGuid()));
    }
}
