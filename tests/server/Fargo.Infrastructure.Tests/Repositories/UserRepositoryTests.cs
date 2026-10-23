using Fargo.Application.Common;
using Fargo.Core.Informations;
using Fargo.Core.Users;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task AnyAsync_WhenEmpty_ShouldReturnFalse()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);

        Assert.False(await repo.AnyAsync());
    }

    [Fact]
    public async Task Add_ThenAnyAsync_ShouldReturnTrue()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);

        repo.Add(User.CreateUser(new Nameid("alice")));
        await context.SaveChangesAsync();

        Assert.True(await repo.AnyAsync());
    }

    [Fact]
    public async Task GetByGuid_ShouldReturnUserWithAggregate()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = User.CreateUser(new Nameid("bob"));
        repo.Add(user);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(user.Guid);

        Assert.NotNull(found);
        Assert.Equal(user.Guid, found!.Guid);
    }

    [Fact]
    public async Task GetByNameid_ShouldBeCaseInsensitive()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(User.CreateUser(new Nameid("carol")));
        await context.SaveChangesAsync();

        var found = await repo.GetByNameidAsync(new Nameid("CAROL"));

        Assert.NotNull(found);
    }

    [Fact]
    public async Task ExistsByNameid_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(User.CreateUser(new Nameid("dave")));
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsByNameidAsync(new Nameid("dave")));
        Assert.False(await repo.ExistsByNameidAsync(new Nameid("nobody")));
    }

    [Fact]
    public async Task GetInfoByGuid_ShouldProjectDto()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = User.CreateUser(new Nameid("erin"));
        repo.Add(user);
        await context.SaveChangesAsync();

        var dto = await repo.GetInfoByGuidAsync(user.Guid);

        Assert.NotNull(dto);
        Assert.Equal(user.Guid, dto!.Guid);
    }

    [Fact]
    public async Task GetManyInfo_ShouldPageAndReportTotal()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(User.CreateUser(new Nameid("usr1")));
        repo.Add(User.CreateUser(new Nameid("usr2")));
        repo.Add(User.CreateUser(new Nameid("usr3")));
        await context.SaveChangesAsync();

        var page = await repo.GetManyInfoAsync(new Pagination(new Page(1), new Limit(2)));

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task Remove_ShouldDeleteUser()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = User.CreateUser(new Nameid("frank"));
        repo.Add(user);
        await context.SaveChangesAsync();

        repo.Remove(user);
        await context.SaveChangesAsync();

        Assert.Null(await repo.GetByGuidAsync(user.Guid));
    }
}
