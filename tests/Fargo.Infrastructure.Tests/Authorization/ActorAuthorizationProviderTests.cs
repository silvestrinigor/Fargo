using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Authorization;
using Fargo.Infrastructure.Caching;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;

namespace Fargo.Infrastructure.Tests.Authorization;

public class ActorAuthorizationProviderTests
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IServiceAccountRepository serviceAccountRepository = Substitute.For<IServiceAccountRepository>();
    private readonly CountingCache cache = new();

    private FargoDbContext CreateContext()
    {
        var context = new FargoDbContext(new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(new AuthorizationStampSaveChangesInterceptor())
            .Options);

        context.Database.EnsureCreated();

        return context;
    }

    // One provider per request scope, as registered in DI.
    private ActorAuthorizationProvider CreateProvider(FargoDbContext context)
        => new(new ActorAuthorizationStateReader(context), cache, userRepository, serviceAccountRepository);

    private async Task<Actor> SaveUserAsync(bool active = true)
    {
        var actor = Actor.NewUser(new Nameid($"user-{Guid.NewGuid():N}"[..20]));

        if (!active)
        {
            actor.Deactivate();
        }

        await using var context = CreateContext();
        context.Actors.Add(actor);
        await context.SaveChangesAsync();

        userRepository
            .GetAllActivePermissionsFromUserAsync(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<ActionType>>([ActionType.CreateArticle]));
        userRepository
            .GetAllActivePartitionAccessGuidsFromUserAsync(actor.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([Guid.Parse("00000000-0000-0000-0000-000000000002")]));

        return actor;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnThePermissionsAndPartitionAccesses()
    {
        var actor = await SaveUserAsync();
        await using var context = CreateContext();

        var authorization = await CreateProvider(context).GetAsync(actor.Guid, ActorType.User);

        Assert.NotNull(authorization);
        Assert.Equal([ActionType.CreateArticle], authorization.Permissions);
        Assert.Single(authorization.PartitionAccessGuids);
    }

    [Fact]
    public async Task GetAsync_WithTheSameStamp_ShouldLoadFromTheDatabaseOnce()
    {
        var actor = await SaveUserAsync();

        for (var request = 0; request < 3; request++)
        {
            await using var context = CreateContext();
            await CreateProvider(context).GetAsync(actor.Guid, ActorType.User);
        }

        await userRepository.Received(1)
            .GetAllActivePermissionsFromUserAsync(actor.Guid, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_AfterAnAuthorizationChange_ShouldLoadAgain()
    {
        var actor = await SaveUserAsync();

        await using (var context = CreateContext())
        {
            await CreateProvider(context).GetAsync(actor.Guid, ActorType.User);
        }

        // Any authorization-relevant change renews the stamp.
        await using (var context = CreateContext())
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("Global")));
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            await CreateProvider(context).GetAsync(actor.Guid, ActorType.User);
        }

        await userRepository.Received(2)
            .GetAllActivePermissionsFromUserAsync(actor.Guid, Arg.Any<CancellationToken>());
        Assert.Equal(2, cache.DistinctKeys.Count);
    }

    [Fact]
    public async Task GetAsync_WhenTheActorIsInactive_ShouldReturnNullWithoutTouchingTheCache()
    {
        var actor = await SaveUserAsync(active: false);
        await using var context = CreateContext();

        Assert.Null(await CreateProvider(context).GetAsync(actor.Guid, ActorType.User));
        Assert.Empty(cache.DistinctKeys);
    }

    [Fact]
    public async Task GetAsync_WhenTheActorDoesNotExist_ShouldReturnNull()
    {
        await using var context = CreateContext();

        Assert.Null(await CreateProvider(context).GetAsync(Guid.NewGuid(), ActorType.User));
    }

    [Fact]
    public async Task GetAsync_WhenTheStampRowIsMissing_ShouldNotCache()
    {
        var actor = await SaveUserAsync();

        await using (var context = CreateContext())
        {
            context.AuthorizationStamps.RemoveRange(context.AuthorizationStamps);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        Assert.NotNull(await CreateProvider(readContext).GetAsync(actor.Guid, ActorType.User));
        Assert.Empty(cache.DistinctKeys);
    }

    private sealed class CountingCache : IActorAuthorizationCache
    {
        private readonly Dictionary<ActorAuthorizationCacheKey, CachedActorAuthorization> entries = [];

        public HashSet<ActorAuthorizationCacheKey> DistinctKeys { get; } = [];

        public async Task<CachedActorAuthorization> GetOrCreateAsync(
            ActorAuthorizationCacheKey key,
            Func<CancellationToken, Task<CachedActorAuthorization>> factory,
            CancellationToken cancellationToken = default)
        {
            DistinctKeys.Add(key);

            if (!entries.TryGetValue(key, out var value))
            {
                value = await factory(cancellationToken);
                entries[key] = value;
            }

            return value;
        }
    }
}

public class HybridActorAuthorizationCacheTests
{
    private static HybridActorAuthorizationCache CreateCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        var provider = services.BuildServiceProvider();

        return new HybridActorAuthorizationCache(
            provider.GetRequiredService<HybridCache>(),
            Options.Create(new ActorAuthorizationCacheOptions()));
    }

    private static readonly CachedActorAuthorization Value = new(
        [ActionType.CreateArticle, ActionType.EditItem],
        [Guid.Parse("10000000-0000-4000-8000-000000000001")]);

    [Fact]
    public async Task GetOrCreateAsync_WithTheSameKey_ShouldRunTheFactoryOnce()
    {
        var cache = CreateCache();
        var key = new ActorAuthorizationCacheKey(ActorType.User, Guid.NewGuid(), Guid.NewGuid());
        var calls = 0;

        for (var i = 0; i < 3; i++)
        {
            await cache.GetOrCreateAsync(key, _ => { calls++; return Task.FromResult(Value); });
        }

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithANewStamp_ShouldRunTheFactoryAgain()
    {
        var cache = CreateCache();
        var actor = Guid.NewGuid();
        var calls = 0;

        await cache.GetOrCreateAsync(new(ActorType.User, actor, Guid.NewGuid()), _ => { calls++; return Task.FromResult(Value); });
        await cache.GetOrCreateAsync(new(ActorType.User, actor, Guid.NewGuid()), _ => { calls++; return Task.FromResult(Value); });

        Assert.Equal(2, calls);
    }

    [Fact]
    public void ToCacheKey_ShouldIncludeTheActorTypeGuidAndStamp()
    {
        var actor = Guid.Parse("a3e9d1c2-0000-4000-8000-000000000002");
        var stamp = Guid.Parse("6f1c2b3a-0000-7000-8000-000000000001");

        Assert.Equal(
            $"fargo:actor-authorization:{(int)ActorType.Service}:{actor:N}:{stamp:N}",
            HybridActorAuthorizationCache.ToCacheKey(new ActorAuthorizationCacheKey(ActorType.Service, actor, stamp)));
    }

    [Fact]
    public void CachedValue_ShouldRoundTripThroughJson_ForADistributedBackend()
    {
        var json = JsonSerializer.Serialize(Value);

        var back = JsonSerializer.Deserialize<CachedActorAuthorization>(json);

        Assert.NotNull(back);
        Assert.Equal(Value.Permissions, back.Permissions);
        Assert.Equal(Value.PartitionAccessGuids, back.PartitionAccessGuids);
    }
}
