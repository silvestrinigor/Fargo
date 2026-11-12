using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Authorization;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Authorization;

public class AuthorizationStampTests
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    private FargoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(new AuthorizationStampSaveChangesInterceptor())
            .Options;

        var context = new FargoDbContext(options);

        // Applies the seeded stamp row, as the migration does.
        context.Database.EnsureCreated();

        return context;
    }

    private async Task<Guid> ReadStampAsync()
    {
        await using var context = CreateContext();

        return await context.AuthorizationStamps.Select(s => s.Value).SingleAsync();
    }

    [Fact]
    public async Task TheSeededStamp_ShouldExist()
    {
        Assert.Equal(AuthorizationStamp.InitialValue, await ReadStampAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenAPartitionIsAdded_ShouldRenewTheStamp()
    {
        await using (var context = CreateContext())
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("Global")));
            await context.SaveChangesAsync();
        }

        Assert.NotEqual(AuthorizationStamp.InitialValue, await ReadStampAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenAnActorChanges_ShouldRenewTheStamp()
    {
        var actor = Actor.NewUser(new Nameid("stamp-user"));

        await using (var context = CreateContext())
        {
            context.Actors.Add(actor);
            await context.SaveChangesAsync();
        }

        var afterCreate = await ReadStampAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Actors.SingleAsync(a => a.Guid == actor.Guid);
            loaded.AddPermission(ActionType.CreateArticle);
            await context.SaveChangesAsync();
        }

        Assert.NotEqual(AuthorizationStamp.InitialValue, afterCreate);
        Assert.NotEqual(afterCreate, await ReadStampAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenOnlyAnArticleChanges_ShouldKeepTheStamp()
    {
        await using (var context = CreateContext())
        {
            context.Articles.Add(Article.NewArticle(new Name("Widget")));
            await context.SaveChangesAsync();
        }

        Assert.Equal(AuthorizationStamp.InitialValue, await ReadStampAsync());
    }

    [Fact]
    public async Task SaveChanges_TwiceInTheSameContext_ShouldRenewTheStampEachTime()
    {
        await using var context = CreateContext();

        var global = Partition.CreateGlobalPartition(new Name("Global"));
        context.Partitions.Add(global);
        await context.SaveChangesAsync();
        var first = await ReadStampAsync();

        context.Partitions.Add(Partition.CreatePartition(new Name("Branch"), global));
        await context.SaveChangesAsync();

        Assert.NotEqual(first, await ReadStampAsync());
    }

    [Fact]
    public async Task StateReader_ShouldReadTheActorFlagsAndTheStamp()
    {
        var actor = Actor.NewUser(new Nameid("state-user"));

        await using (var context = CreateContext())
        {
            context.Actors.Add(actor);
            await context.SaveChangesAsync();
        }

        var stamp = await ReadStampAsync();

        await using var readContext = CreateContext();
        var state = await new ActorAuthorizationStateReader(readContext).GetAsync(actor.Guid, ActorType.User);

        Assert.NotNull(state);
        Assert.Equal(actor.Guid, state.ActorGuid);
        Assert.True(state.IsActive);
        Assert.Equal(actor.User!.Authentication.AuthVersion, state.AuthVersion);
        Assert.Equal(stamp, state.AuthorizationStamp);
    }

    [Fact]
    public async Task StateReader_WhenTheActorDoesNotExist_ShouldReturnNull()
    {
        await using var context = CreateContext();

        Assert.Null(await new ActorAuthorizationStateReader(context).GetAsync(Guid.NewGuid(), ActorType.User));
        Assert.Null(await new ActorAuthorizationStateReader(context).GetAsync(Guid.NewGuid(), ActorType.Service));
    }

    [Fact]
    public async Task StateReader_ShouldReadOncePerScope()
    {
        var actor = Actor.NewUser(new Nameid("memo-user"));

        await using (var context = CreateContext())
        {
            context.Actors.Add(actor);
            await context.SaveChangesAsync();
        }

        await using var readContext = CreateContext();
        var reader = new ActorAuthorizationStateReader(readContext);
        var first = await reader.GetAsync(actor.Guid, ActorType.User);

        await using (var context = CreateContext())
        {
            (await context.Actors.SingleAsync(a => a.Guid == actor.Guid)).Deactivate();
            await context.SaveChangesAsync();
        }

        // Same scope: the first read is reused; a new scope sees the change.
        Assert.Same(first, await reader.GetAsync(actor.Guid, ActorType.User));
        Assert.False((await new ActorAuthorizationStateReader(readContext).GetAsync(actor.Guid, ActorType.User))!.IsActive);
    }
}

/// <summary>
/// The state queries run on every authenticated request. Translating them with the real Npgsql
/// provider (no database needed) proves they become one SQL statement, with the stamp as a subquery.
/// </summary>
public class AuthorizationStateQueryTranslationTests
{
    private static FargoDbContext CreateNpgsqlContext()
        => new(new DbContextOptionsBuilder<FargoDbContext>()
            .UseNpgsql("Host=localhost;Database=unused")
            .UseSnakeCaseNamingConvention()
            .Options);

    [Fact]
    public void UserStateQuery_ShouldTranslateToASingleStatementWithTheStamp()
    {
        using var context = CreateNpgsqlContext();

        var sql = ActorAuthorizationStateReader.UserStateQuery(context, Guid.NewGuid()).ToQueryString();

        Assert.Contains("authorization_stamps", sql);
        Assert.Contains("auth_version", sql);
        Assert.Contains("is_active", sql);
    }

    [Fact]
    public void ServiceAccountStateQuery_ShouldTranslateToASingleStatementWithTheStamp()
    {
        using var context = CreateNpgsqlContext();

        var sql = ActorAuthorizationStateReader.ServiceAccountStateQuery(context, Guid.NewGuid()).ToQueryString();

        Assert.Contains("authorization_stamps", sql);
        Assert.Contains("auth_version", sql);
        Assert.Contains("is_active", sql);
    }
}
