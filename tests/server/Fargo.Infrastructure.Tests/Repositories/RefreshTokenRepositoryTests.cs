using Fargo.Core.Identity;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Tests.Common;

namespace Fargo.Infrastructure.Tests.Repositories;

public class RefreshTokenRepositoryTests
{
    private static TokenHash Hash(string value) => new(value);

    [Fact]
    public async Task Add_ThenGetByGuid_ShouldReturnToken()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);
        var token = RefreshToken.Create(Guid.NewGuid(), Hash("HASH-A"));

        repo.Add(token);
        await context.SaveChangesAsync();

        var found = await repo.GetByGuidAsync(token.Guid);

        Assert.NotNull(found);
        Assert.Equal(token.Guid, found!.Guid);
    }

    [Fact]
    public async Task GetByTokenHash_ShouldReturnMatchingToken()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);
        var token = RefreshToken.Create(Guid.NewGuid(), Hash("HASH-B"));

        repo.Add(token);
        await context.SaveChangesAsync();

        var found = await repo.GetByTokenHashAsync(Hash("HASH-B"));

        Assert.NotNull(found);
        Assert.Equal(token.Guid, found!.Guid);
    }

    [Fact]
    public async Task GetByUserGuid_ShouldReturnOnlyThatUsersTokens()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        repo.Add(RefreshToken.Create(userA, Hash("A1")));
        repo.Add(RefreshToken.Create(userA, Hash("A2")));
        repo.Add(RefreshToken.Create(userB, Hash("B1")));
        await context.SaveChangesAsync();

        var tokens = await repo.GetByUserGuidAsync(userA);

        Assert.Equal(2, tokens.Count);
        Assert.All(tokens, t => Assert.Equal(userA, t.UserGuid));
    }

    [Fact]
    public async Task GetByGuid_WhenMissing_ShouldReturnNull()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);

        var found = await repo.GetByGuidAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task Remove_ShouldDeleteToken()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);
        var token = RefreshToken.Create(Guid.NewGuid(), Hash("HASH-C"));
        repo.Add(token);
        await context.SaveChangesAsync();

        repo.Remove(token);
        await context.SaveChangesAsync();

        Assert.Null(await repo.GetByGuidAsync(token.Guid));
    }

    [Fact]
    public async Task RevokedTokenPersistsRevocation()
    {
        await using var context = TestDbContextFactory.Create();
        var repo = new RefreshTokenRepository(context);
        var token = RefreshToken.Create(Guid.NewGuid(), Hash("HASH-D"));
        repo.Add(token);
        await context.SaveChangesAsync();

        token.Revoke();
        await context.SaveChangesAsync();

        var reloaded = await repo.GetByGuidAsync(token.Guid);
        Assert.NotNull(reloaded);
        Assert.NotNull(reloaded!.RevokedAt);
        Assert.False(reloaded.IsUsable);
    }
}
