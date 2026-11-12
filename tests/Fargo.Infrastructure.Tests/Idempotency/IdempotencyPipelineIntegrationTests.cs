using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Idempotency;
using Fargo.Application.Commands.Items;
using Fargo.Application.Common.Actors;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Idempotency;
using Fargo.Application.Persistence;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fargo.Infrastructure.Tests.Idempotency;

/// <summary>
/// The whole idempotency pipeline against real PostgreSQL: the real decorators, unit of work, repositories and
/// audit log, with a fake command handler so only the idempotency behaviour is under test.
/// </summary>
public class IdempotencyPipelineIntegrationTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private Guid actorGuid;

    public async Task InitializeAsync()
    {
        await using var context = postgres.CreateContext();

        await context.Database.ExecuteSqlRawAsync("TRUNCATE idempotency_keys, audit_logs");

        if (!await context.Partitions.AnyAsync(p => p.Guid == FargoCoreWellKnowGuids.GlobalPartitionGuid))
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("global")));
        }

        var actor = Actor.NewUser(new Nameid($"actor-{Guid.NewGuid():N}"[..20]));

        context.Actors.Add(actor);

        await context.SaveChangesAsync();

        actorGuid = actor.Guid;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private ServiceProvider BuildProvider(FakeItemCreateHandler handler)
    {
        var services = new ServiceCollection();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new IdempotencyOptions()));
        services.AddScoped(_ => postgres.CreateContext());
        services.AddScoped<IUnitOfWork>(sp => new UnitOfWork(sp.GetRequiredService<FargoDbContext>(), NullLogger<IUnitOfWork>.Instance));
        services.AddScoped<IAuditLogRepository>(sp => new AuditLogRepository(sp.GetRequiredService<FargoDbContext>()));
        services.AddScoped<IIdempotencyRecordRepository>(sp => new IdempotencyRecordRepository(sp.GetRequiredService<FargoDbContext>()));
        services.AddScoped<ICurrentActor>(_ => new TestActor(actorGuid));
        services.AddSingleton(handler);
        services.AddFargoIdempotencyApplication();
        services.AddFargoCommandHandler<ItemCreateCommand, Guid, FakeItemCreateHandler>();

        return services.BuildServiceProvider();
    }

    private static readonly Guid Key1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private static async Task<Guid> SendAsync(IServiceProvider provider, Guid? key, Guid? articleGuid = null)
    {
        await using var scope = provider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ItemCreateCommand, Guid>>();

        return await handler.HandleAsync(new ItemCreateCommand(new ItemCreateDto(articleGuid ?? Guid.Empty), key));
    }

    private async Task<(int Records, int AuditLogs)> CountsAsync()
    {
        await using var context = postgres.CreateContext();

        return (await context.IdempotencyRecords.CountAsync(), await context.AuditLogs.CountAsync());
    }

    [Fact]
    public async Task ARetryWithTheSameKey_ShouldReturnTheFirstResultAndLeaveOneRecordAndOneAuditLog()
    {
        var handler = new FakeItemCreateHandler();
        await using var provider = BuildProvider(handler);

        var first = await SendAsync(provider, Key1);
        var second = await SendAsync(provider, Key1);

        Assert.Equal(first, second);
        Assert.Equal(1, handler.Calls);
        Assert.Equal((1, 1), await CountsAsync());
    }

    [Fact]
    public async Task WithoutAKey_EveryRequestShouldRunAndNothingShouldBeStored()
    {
        var handler = new FakeItemCreateHandler();
        await using var provider = BuildProvider(handler);

        var first = await SendAsync(provider, null);
        var second = await SendAsync(provider, null);

        Assert.NotEqual(first, second);
        Assert.Equal(2, handler.Calls);
        Assert.Equal((0, 2), await CountsAsync());
    }

    [Fact]
    public async Task ARequestThatFails_ShouldLeaveNoRecordSoARetryRunsAgain()
    {
        var handler = new FakeItemCreateHandler { FailNext = true };
        await using var provider = BuildProvider(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() => SendAsync(provider, Key1));
        Assert.Equal((0, 0), await CountsAsync());

        await SendAsync(provider, Key1);

        Assert.Equal(2, handler.Calls);
        Assert.Equal((1, 1), await CountsAsync());
    }

    [Fact]
    public async Task TwoRequestsWithTheSameKeyAtTheSameTime_ShouldCommitOnlyOne()
    {
        // Both requests find no record and both run the handler; only the first save can commit, and the loser's
        // audit log goes with it, because the record and the changes are saved together.
        var handler = new FakeItemCreateHandler { MeetBeforeFinishing = new AsyncMeeting(2) };
        await using var provider = BuildProvider(handler);

        var requests = new[]
        {
            Task.Run(() => SendAsync(provider, Key1)),
            Task.Run(() => SendAsync(provider, Key1)),
        };

        await Task.WhenAll(requests.Select(async request =>
        {
            try
            {
                await request;
            }
            catch (DbUpdateException)
            {
                // The loser: the unique key rejected its record.
            }
        }));

        Assert.Equal(1, requests.Count(r => r.IsCompletedSuccessfully));
        var failed = Assert.Single(requests, r => r.IsFaulted);
        var exception = Assert.IsType<DbUpdateException>(failed.Exception!.InnerException);
        Assert.Contains(IdempotencyRecord.PrimaryKeyName, exception.InnerException!.Message);
        Assert.Equal(2, handler.Calls);
        Assert.Equal((1, 1), await CountsAsync());
    }

    [Fact]
    public async Task AfterTheLoserRetries_ItShouldReceiveTheWinnersResult()
    {
        var handler = new FakeItemCreateHandler { MeetBeforeFinishing = new AsyncMeeting(2) };
        await using var provider = BuildProvider(handler);

        var requests = new[]
        {
            Task.Run(() => SendAsync(provider, Key1)),
            Task.Run(() => SendAsync(provider, Key1)),
        };

        await Task.WhenAny(requests);
        await Task.WhenAll(requests.Select(r => r.ContinueWith(_ => { })));

        var winner = requests.Single(r => r.IsCompletedSuccessfully).Result;

        var retried = await SendAsync(provider, Key1);

        Assert.Equal(winner, retried);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task APurgeAfterTheKeyExpires_ShouldRemoveItAndLetTheKeyBeUsedAgain()
    {
        var handler = new FakeItemCreateHandler();
        await using var provider = BuildProvider(handler);
        await SendAsync(provider, Key1);

        await using (var scope = provider.CreateAsyncScope())
        {
            var purge = scope.ServiceProvider
                .GetRequiredService<ICommandHandler<PurgeExpiredIdempotencyKeysCommand, int>>();

            Assert.Equal(0, await purge.HandleAsync(new PurgeExpiredIdempotencyKeysCommand(DateTimeOffset.UtcNow)));
            Assert.Equal(1, await purge.HandleAsync(new PurgeExpiredIdempotencyKeysCommand(DateTimeOffset.UtcNow.AddDays(2))));
        }

        var (records, _) = await CountsAsync();
        Assert.Equal(0, records);

        await SendAsync(provider, Key1);

        Assert.Equal(2, handler.Calls);
    }

    private sealed class TestActor(Guid guid) : ICurrentActor
    {
        public Guid Guid { get; } = guid;

        public ActorType ActorType => ActorType.User;

        public bool IsAuthenticated => true;
    }

    private sealed class FakeItemCreateHandler : ICommandHandler<ItemCreateCommand, Guid>
    {
        private int calls;

        public int Calls => Volatile.Read(ref calls);

        public bool FailNext { get; set; }

        public AsyncMeeting? MeetBeforeFinishing { get; init; }

        public async Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref calls);

            if (FailNext)
            {
                FailNext = false;

                throw new InvalidOperationException("The command failed.");
            }

            if (MeetBeforeFinishing is not null)
            {
                await MeetBeforeFinishing.ArriveAsync();
            }

            return Guid.NewGuid();
        }
    }

    /// <summary>
    /// Holds each caller until the expected number have arrived, so the requests overlap.
    /// </summary>
    private sealed class AsyncMeeting(int participants)
    {
        private readonly TaskCompletionSource released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrived;

        public Task ArriveAsync()
        {
            if (Interlocked.Increment(ref arrived) >= participants)
            {
                released.TrySetResult();
            }

            return released.Task.WaitAsync(TimeSpan.FromSeconds(15));
        }
    }
}
