using Fargo.Application.Idempotency;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Repositories;

/// <summary>
/// Idempotency storage against real PostgreSQL: the unique key and the <c>xmin</c> token do the work that keeps
/// a retried request from running twice, and the purge uses a PostgreSQL-specific <c>ctid</c> delete.
/// </summary>
public class IdempotencyRecordRepositoryTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public async Task InitializeAsync()
    {
        await using var context = postgres.CreateContext();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE idempotency_keys");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Records belong to an actor, which must exist, and every user belongs to the global partition.
    private async Task<Guid> CreateActorAsync()
    {
        await using var context = postgres.CreateContext();

        if (!await context.Partitions.AnyAsync(p => p.Guid == FargoCoreWellKnowGuids.GlobalPartitionGuid))
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("global")));
        }

        var actor = Actor.NewUser(new Nameid($"actor-{Guid.NewGuid():N}"[..20]));

        context.Actors.Add(actor);

        await context.SaveChangesAsync();

        return actor.Guid;
    }

    private static readonly Guid Key1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static IdempotencyRecord NewRecord(Guid actorGuid, Guid key, TimeSpan retention, string? response = null)
        => IdempotencyRecord.Create(actorGuid, key, "ItemCreateCommand", response, Now, retention);

    private async Task AddAsync(params IdempotencyRecord[] records)
    {
        await using var context = postgres.CreateContext();

        foreach (var record in records)
        {
            new IdempotencyRecordRepository(context).Add(record);
        }

        await context.SaveChangesAsync();
    }

    private async Task<int> CountAsync()
    {
        await using var context = postgres.CreateContext();

        return await context.IdempotencyRecords.CountAsync();
    }

    [Fact]
    public async Task AddThenFind_ShouldRoundTripTheRecord()
    {
        var actorGuid = await CreateActorAsync();
        var guid = Guid.NewGuid().ToString();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(24), response: $"\"{guid}\""));

        await using var context = postgres.CreateContext();
        var found = await new IdempotencyRecordRepository(context).FindAsync(actorGuid, Key1);

        Assert.NotNull(found);
        Assert.Equal("ItemCreateCommand", found!.CommandName);
                Assert.Equal($"\"{guid}\"", found.Response);
        Assert.Equal(Now + TimeSpan.FromHours(24), found.ExpiresAt);
    }

    [Fact]
    public async Task Find_ShouldBeScopedToTheActorAndTheKey()
    {
        var actorGuid = await CreateActorAsync();
        var otherActorGuid = await CreateActorAsync();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(24)));

        await using var context = postgres.CreateContext();
        var repository = new IdempotencyRecordRepository(context);

        Assert.NotNull(await repository.FindAsync(actorGuid, Key1));
        Assert.Null(await repository.FindAsync(actorGuid, Guid.NewGuid()));
        Assert.Null(await repository.FindAsync(otherActorGuid, Key1));
    }

    [Fact]
    public async Task TheSameKeyForDifferentActors_ShouldBothBeStored()
    {
        var first = await CreateActorAsync();
        var second = await CreateActorAsync();

        await AddAsync(NewRecord(first, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), TimeSpan.FromHours(24)), NewRecord(second, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), TimeSpan.FromHours(24)));

        Assert.Equal(2, await CountAsync());
    }

    [Fact]
    public async Task AddingTheSameKeyTwice_ShouldFailOnTheUniqueKeyAndNameIt()
    {
        // This is what makes only one of two racing requests commit. The HTTP layer turns the failure into a
        // 409 by finding the constraint name in the message, so the name must be there.
        var actorGuid = await CreateActorAsync();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(24)));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(24))));

        Assert.Contains(IdempotencyRecord.PrimaryKeyName, exception.InnerException!.Message);
        Assert.Equal(1, await CountAsync());
    }

    [Fact]
    public async Task RenewingAnExpiredRecordTwiceAtOnce_ShouldLetOnlyOneSaveWin()
    {
        var actorGuid = await CreateActorAsync();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(-1)));

        await using var firstContext = postgres.CreateContext();
        await using var secondContext = postgres.CreateContext();
        var first = await new IdempotencyRecordRepository(firstContext).FindAsync(actorGuid, Key1);
        var second = await new IdempotencyRecordRepository(secondContext).FindAsync(actorGuid, Key1);

        first!.Renew("ItemCreateCommand", "\"first\"", Now, TimeSpan.FromHours(24));
        second!.Renew("ItemCreateCommand", "\"second\"", Now, TimeSpan.FromHours(24));

        await firstContext.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());
    }

    [Fact]
    public async Task PurgeExpired_ShouldDeleteOnlyRecordsThatExpiredBeforeNow()
    {
        var actorGuid = await CreateActorAsync();
        var validKey = Guid.NewGuid();
        await AddAsync(
            NewRecord(actorGuid, Guid.NewGuid(), TimeSpan.FromHours(1)),
            NewRecord(actorGuid, Guid.NewGuid(), TimeSpan.FromHours(2)),
            NewRecord(actorGuid, validKey, TimeSpan.FromHours(24)));

        await using var context = postgres.CreateContext();
        var deleted = await new IdempotencyRecordRepository(context).PurgeExpiredAsync(Now + TimeSpan.FromHours(3), 100);

        Assert.Equal(2, deleted);

        await using var verify = postgres.CreateContext();
        var remaining = await verify.IdempotencyRecords.Select(r => r.Key).ToListAsync();

        Assert.Equal([validKey], remaining);
    }

    [Fact]
    public async Task PurgeExpired_ShouldNotDeleteARecordAtTheExactMomentItExpires()
    {
        var actorGuid = await CreateActorAsync();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(1)));

        await using var context = postgres.CreateContext();
        var deleted = await new IdempotencyRecordRepository(context).PurgeExpiredAsync(Now + TimeSpan.FromHours(1), 100);

        Assert.Equal(0, deleted);
    }

    [Fact]
    public async Task PurgeExpired_ShouldDeleteNoMoreThanTheBatchSize()
    {
        var actorGuid = await CreateActorAsync();
        await AddAsync([.. Enumerable.Range(0, 5).Select(_ => NewRecord(actorGuid, Guid.NewGuid(), TimeSpan.FromHours(1)))]);

        await using var context = postgres.CreateContext();
        var deleted = await new IdempotencyRecordRepository(context).PurgeExpiredAsync(Now + TimeSpan.FromDays(1), 2);

        Assert.Equal(2, deleted);
        Assert.Equal(3, await CountAsync());
    }

    [Fact]
    public async Task DeletingTheActor_ShouldDeleteItsRecords()
    {
        var actorGuid = await CreateActorAsync();
        await AddAsync(NewRecord(actorGuid, Key1, TimeSpan.FromHours(24)));

        await using (var context = postgres.CreateContext())
        {
            await context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM actors WHERE guid = {actorGuid}");
        }

        Assert.Equal(0, await CountAsync());
    }
}
