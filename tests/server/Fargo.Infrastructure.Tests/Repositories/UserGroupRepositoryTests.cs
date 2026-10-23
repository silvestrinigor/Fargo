using Fargo.Application.Common;
using Fargo.Core.Informations;
using Fargo.Core.UserGroups;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class UserGroupRepositoryTests
{
    [Fact]
    public async Task Add_ThenGetByGuid_ShouldReturnGroup()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);
        var group = UserGroup.CreateUserGroup(new Nameid("editors"));
        repo.Add(group);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(group.Guid);

        Assert.NotNull(found);
        Assert.Equal(group.Guid, found!.Guid);
    }

    [Fact]
    public async Task ExistsByNameid_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);
        repo.Add(UserGroup.CreateUserGroup(new Nameid("viewers")));
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsByNameidAsync(new Nameid("viewers")));
        Assert.False(await repo.ExistsByNameidAsync(new Nameid("ghosts")));
    }

    [Fact]
    public async Task HasChildrenUserGroup_ShouldReflectParentAssignment()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);
        var parent = UserGroup.CreateUserGroup(new Nameid("parent"));
        var child = UserGroup.CreateUserGroup(new Nameid("child"));
        child.SetParentUserGroup(parent);
        repo.Add(parent);
        repo.Add(child);
        await context.SaveChangesAsync();

        Assert.True(await repo.HasChildrenUserGroupAsync(parent.Guid));
        Assert.False(await repo.HasChildrenUserGroupAsync(child.Guid));
    }

    [Fact]
    public async Task GetInfoByGuid_ShouldProjectDto()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);
        var group = UserGroup.CreateUserGroup(new Nameid("ops"));
        repo.Add(group);
        await context.SaveChangesAsync();

        var dto = await repo.GetInfoByGuidAsync(group.Guid);

        Assert.NotNull(dto);
        Assert.Equal(group.Guid, dto!.Guid);
    }

    [Fact]
    public async Task GetManyInfo_ShouldPageAndReportTotal()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);
        repo.Add(UserGroup.CreateUserGroup(new Nameid("grp1")));
        repo.Add(UserGroup.CreateUserGroup(new Nameid("grp2")));
        await context.SaveChangesAsync();

        var page = await repo.GetManyInfoAsync(new Pagination(new Page(1), new Limit(50)));

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task AnyAsync_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserGroupRepository(context);

        Assert.False(await repo.AnyAsync());

        repo.Add(UserGroup.CreateUserGroup(new Nameid("some")));
        await context.SaveChangesAsync();

        Assert.True(await repo.AnyAsync());
    }
}
