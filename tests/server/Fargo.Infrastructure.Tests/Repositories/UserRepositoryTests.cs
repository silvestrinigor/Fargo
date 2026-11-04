using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Informations;
using Fargo.Core.Entities.Actors.Users;
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

        repo.Add(Actor.NewUser(new Nameid("alice")).User!);
        await context.SaveChangesAsync();

        Assert.True(await repo.AnyAsync());
    }

    [Fact]
    public async Task GetByGuid_ShouldReturnUserWithAggregate()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = Actor.NewUser(new Nameid("bob")).User!;
        repo.Add(user);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(user.ActorGuid);

        Assert.NotNull(found);
        Assert.Equal(user.ActorGuid, found!.ActorGuid);
    }

    [Fact]
    public async Task GetByGuid_AfterReload_ShouldLoadActorAndAuthentication()
    {
        // Reproduces exactly what the JWT OnTokenValidated path touches: load the
        // user by actor guid in a FRESH context and read Actor.IsActive and
        // Authentication.AuthVersion.
        var dbName = Guid.NewGuid().ToString();

        Guid actorGuid;
        Guid authVersion;

        await using (var seedContext = TestDbContextFactory.Create(dbName))
        {
            var seedRepo = new UserRepository(seedContext);
            var created = Actor.NewAdministratorUser(new Nameid("admin")).User!;
            actorGuid = created.ActorGuid;
            authVersion = created.Authentication.AuthVersion;
            seedRepo.Add(created);
            await seedContext.SaveChangesAsync();
        }

        await using var readContext = TestDbContextFactory.Create(dbName);
        var readRepo = new UserRepository(readContext);

        var found = await readRepo.GetByGuidAsync(actorGuid);

        Assert.NotNull(found);
        Assert.NotNull(found!.Actor);
        Assert.True(found.Actor.IsActive);
        Assert.Equal(authVersion, found.Authentication.AuthVersion);
    }

    [Fact]
    public async Task GetByNameid_ShouldBeCaseInsensitive()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(Actor.NewUser(new Nameid("carol")).User!);
        await context.SaveChangesAsync();

        var found = await repo.GetByNameidAsync(new Nameid("CAROL"));

        Assert.NotNull(found);
    }

    [Fact]
    public async Task ExistsByNameid_ShouldReflectPersistence()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(Actor.NewUser(new Nameid("dave")).User!);
        await context.SaveChangesAsync();

        Assert.True(await repo.ExistsByNameidAsync(new Nameid("dave")));
        Assert.False(await repo.ExistsByNameidAsync(new Nameid("nobody")));
    }

    [Fact]
    public async Task GetInfoByGuid_ShouldProjectDto()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        var user = Actor.NewUser(new Nameid("erin")).User!;
        repo.Add(user);
        await context.SaveChangesAsync();

        var dto = await repo.GetInfoByGuidAsync(user.ActorGuid);

        Assert.NotNull(dto);
        Assert.Equal(user.ActorGuid, dto!.Guid);
    }

    [Fact(Skip = "Ordered list paging over a TPT hierarchy is not supported by the EF Core InMemory provider (throws 'At least one object must implement IComparable' while shaping the base/derived join). Covered by the PostgreSQL integration checks instead.")]
    public async Task GetManyInfo_ShouldPageAndReportTotal()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new UserRepository(context);
        repo.Add(Actor.NewUser(new Nameid("usr1")).User!);
        repo.Add(Actor.NewUser(new Nameid("usr2")).User!);
        repo.Add(Actor.NewUser(new Nameid("usr3")).User!);
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
        var user = Actor.NewUser(new Nameid("frank")).User!;
        repo.Add(user);
        await context.SaveChangesAsync();

        repo.Remove(user);
        await context.SaveChangesAsync();

        Assert.Null(await repo.GetByGuidAsync(user.ActorGuid));
    }
}
