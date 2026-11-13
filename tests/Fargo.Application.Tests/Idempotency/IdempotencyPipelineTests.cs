using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Items;
using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Application.Idempotency;
using Fargo.Application.Persistence;
using Fargo.Application.Persistence.Repositories;
using Fargo.Application.Tests.Common;
using Fargo.Core.Audits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Application.Tests.Idempotency;

/// <summary>
/// The idempotency decorator as the host wires it, with real item create and supply order receive commands and
/// counting fake handlers.
/// </summary>
public class IdempotencyPipelineTests
{
    private static readonly Guid Key1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Key2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private readonly InMemoryIdempotencyRecordRepository records = new();
    private readonly IAuditLogRepository auditLogRepository = Substitute.For<IAuditLogRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CountingItemCreateHandler itemCreateHandler = new();
    private readonly CountingReceiveHandler receiveHandler = new();
    private readonly MutableTimeProvider timeProvider = new();
    private readonly IdempotencyOptions options = new() { Retention = TimeSpan.FromHours(24) };
    private readonly ServiceProvider provider;

    private ICurrentActor currentActor = new TestCurrentActor { IsAuthenticated = true };

    public IdempotencyPipelineTests()
    {
        unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task>>()!(CancellationToken.None));

        var services = new ServiceCollection();
        services.AddSingleton(unitOfWork);
        services.AddSingleton(auditLogRepository);
        services.AddSingleton<IIdempotencyRecordRepository>(records);
        services.AddSingleton<TimeProvider>(timeProvider);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddScoped(_ => currentActor);
        services.AddSingleton(itemCreateHandler);
        services.AddSingleton(receiveHandler);
        services.AddFargoIdempotencyApplication();
        services.AddFargoCommandHandler<ItemCreateCommand, Guid, CountingItemCreateHandler>();
        services.AddFargoCommandHandler<SupplyOrderReceiveCommand, CountingReceiveHandler>();

        provider = services.BuildServiceProvider();
    }

    private static ItemCreateCommand CreateItem(Guid? key, Guid? articleGuid = null)
        => new(new ItemCreateDto(articleGuid ?? Guid.Empty), key);

    private static SupplyOrderReceiveCommand Receive(Guid orderGuid, Guid? key, double quantity = 5)
        => new(
            orderGuid,
            new SupplyOrderReceiveDto([new SupplyOrderReceiveLineDto(Guid.Empty, quantity.Amount())]),
            key);

    // One scope per call, as one per HTTP request.
    private async Task<(Guid Result, bool Replayed)> SendCreateAsync(ItemCreateCommand command)
    {
        await using var scope = provider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<ItemCreateCommand, Guid>>();
        var result = await handler.HandleAsync(command);

        return (result, scope.ServiceProvider.GetRequiredService<IdempotencyOutcome>().Replayed);
    }

    private async Task<bool> SendReceiveAsync(SupplyOrderReceiveCommand command)
    {
        await using var scope = provider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<SupplyOrderReceiveCommand>>();
        await handler.HandleAsync(command);

        return scope.ServiceProvider.GetRequiredService<IdempotencyOutcome>().Replayed;
    }

    [Fact]
    public async Task WithoutAKey_ShouldRunEveryTimeAndStoreNothing()
    {
        await SendCreateAsync(CreateItem(null));
        await SendCreateAsync(CreateItem(null));

        Assert.Equal(2, itemCreateHandler.Calls);
        Assert.Empty(records.All);
    }

    [Fact]
    public async Task WithAKey_AFirstRunShouldExecuteAndStoreTheResult()
    {
        var (result, replayed) = await SendCreateAsync(CreateItem(Key1));

        Assert.False(replayed);
        Assert.Equal(1, itemCreateHandler.Calls);
        var record = Assert.Single(records.All);
        Assert.Equal(Key1, record.Key);
        Assert.Equal(nameof(ItemCreateCommand), record.CommandName);
        Assert.Contains(result.ToString(), record.Response);
    }

    [Fact]
    public async Task WithTheSameKey_ARetryShouldReturnTheFirstResultWithoutRunningAgain()
    {
        var (first, _) = await SendCreateAsync(CreateItem(Key1));
        var (second, replayed) = await SendCreateAsync(CreateItem(Key1));

        Assert.Equal(first, second);
        Assert.True(replayed);
        Assert.Equal(1, itemCreateHandler.Calls);
        Assert.Single(records.All);
    }

    [Fact]
    public async Task ARetry_ShouldNotBeAuditedAgain()
    {
        await SendCreateAsync(CreateItem(Key1));
        await SendCreateAsync(CreateItem(Key1));
        await SendCreateAsync(CreateItem(Key1));

        auditLogRepository.Received(1).Add(Arg.Any<AuditLog>());
    }

    [Fact]
    public async Task WithTheSameKeyAndADifferentRequest_ShouldReplayTheFirstResult()
    {
        // The key identifies the operation, not the payload: a client that changes the request uses a new key.
        var (first, _) = await SendCreateAsync(CreateItem(Key1, Guid.NewGuid()));
        var (second, replayed) = await SendCreateAsync(CreateItem(Key1, Guid.NewGuid()));

        Assert.True(replayed);
        Assert.Equal(first, second);
        Assert.Equal(1, itemCreateHandler.Calls);
    }

    [Fact]
    public async Task ADifferentKey_ShouldRunTheCommandAgain()
    {
        var (first, _) = await SendCreateAsync(CreateItem(Key1));
        var (second, replayed) = await SendCreateAsync(CreateItem(Key2));

        Assert.False(replayed);
        Assert.NotEqual(first, second);
        Assert.Equal(2, itemCreateHandler.Calls);
        Assert.Equal(2, records.All.Count);
    }

    [Fact]
    public async Task WithTheSameKeyOnAnotherCommand_ShouldBeRejected()
    {
        await SendCreateAsync(CreateItem(Key1));

        var exception = await Assert.ThrowsAsync<IdempotencyKeyReusedFargoApplicationException>(
            () => SendReceiveAsync(Receive(Guid.NewGuid(), Key1)));

        Assert.Equal(Key1, exception.Key);
        Assert.Equal(0, receiveHandler.Calls);
    }

    [Fact]
    public async Task TheSameKeyFromAnotherActor_ShouldBeIndependent()
    {
        var (Result, Replayed) = await SendCreateAsync(CreateItem(Key1));

        currentActor = new TestCurrentActor { IsAuthenticated = true };
        var second = await SendCreateAsync(CreateItem(Key1));

        Assert.False(second.Replayed);
        Assert.NotEqual(Result, second.Result);
        Assert.Equal(2, itemCreateHandler.Calls);
        Assert.Equal(2, records.All.Count);
    }

    [Fact]
    public async Task AfterTheKeyExpires_TheRequestShouldRunAgainAndRenewTheRecord()
    {
        await SendCreateAsync(CreateItem(Key1));
        var first = Assert.Single(records.All).ExpiresAt;

        timeProvider.Now += options.Retention + TimeSpan.FromMinutes(1);
        var (_, replayed) = await SendCreateAsync(CreateItem(Key1));

        Assert.False(replayed);
        Assert.Equal(2, itemCreateHandler.Calls);
        var record = Assert.Single(records.All);
        Assert.True(record.ExpiresAt > first);
    }

    [Fact]
    public async Task BeforeTheKeyExpires_TheRetryShouldStillBeReplayed()
    {
        await SendCreateAsync(CreateItem(Key1));

        timeProvider.Now += options.Retention - TimeSpan.FromMinutes(1);
        var (_, replayed) = await SendCreateAsync(CreateItem(Key1));

        Assert.True(replayed);
        Assert.Equal(1, itemCreateHandler.Calls);
    }

    [Fact]
    public async Task WhenTheCommandFails_NothingShouldBeStoredAndARetryShouldRunAgain()
    {
        itemCreateHandler.FailNext = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => SendCreateAsync(CreateItem(Key1)));

        Assert.Empty(records.All);

        var (_, replayed) = await SendCreateAsync(CreateItem(Key1));

        Assert.False(replayed);
        Assert.Equal(2, itemCreateHandler.Calls);
        Assert.Single(records.All);
    }

    [Fact]
    public async Task WithoutAnAuthenticatedActor_TheKeyShouldBeIgnored()
    {
        currentActor = new TestCurrentActor { IsAuthenticated = false };

        await SendCreateAsync(CreateItem(Key1));
        await SendCreateAsync(CreateItem(Key1));

        Assert.Equal(2, itemCreateHandler.Calls);
        Assert.Empty(records.All);
    }

    [Fact]
    public async Task TheEmptyGuid_ShouldBeRejectedBeforeTheCommandRuns()
    {
        await Assert.ThrowsAsync<FargoApplicationException>(() => SendCreateAsync(CreateItem(Guid.Empty)));

        Assert.Equal(0, itemCreateHandler.Calls);
    }

    [Fact]
    public async Task AReceive_WithTheSameKey_ShouldBeCountedOnce()
    {
        // The case that motivated the feature: a retried receive used to register the quantity twice.
        var orderGuid = Guid.NewGuid();

        var first = await SendReceiveAsync(Receive(orderGuid, Key1));
        var second = await SendReceiveAsync(Receive(orderGuid, Key1));

        Assert.False(first);
        Assert.True(second);
        Assert.Equal(1, receiveHandler.Calls);
    }

    [Fact]
    public async Task AReceive_WithADifferentQuantityUnderTheSameKey_ShouldReplayAndNotReceiveAgain()
    {
        var orderGuid = Guid.NewGuid();
        await SendReceiveAsync(Receive(orderGuid, Key1, quantity: 5));

        var replayed = await SendReceiveAsync(Receive(orderGuid, Key1, quantity: 6));

        Assert.True(replayed);
        Assert.Equal(1, receiveHandler.Calls);
    }

    [Fact]
    public async Task AReceive_WithADifferentKey_ShouldBeReceivedAgain()
    {
        var orderGuid = Guid.NewGuid();

        await SendReceiveAsync(Receive(orderGuid, Key1));
        await SendReceiveAsync(Receive(orderGuid, Key2));

        Assert.Equal(2, receiveHandler.Calls);
    }

    [Fact]
    public async Task AReceive_WithoutAKey_ShouldRunEveryTime()
    {
        var orderGuid = Guid.NewGuid();

        await SendReceiveAsync(Receive(orderGuid, null));
        await SendReceiveAsync(Receive(orderGuid, null));

        Assert.Equal(2, receiveHandler.Calls);
    }

    private sealed class CountingItemCreateHandler : ICommandHandler<ItemCreateCommand, Guid>
    {
        public int Calls { get; private set; }

        public bool FailNext { get; set; }

        public Task<Guid> HandleAsync(ItemCreateCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;

            if (FailNext)
            {
                FailNext = false;

                throw new InvalidOperationException("The command failed.");
            }

            return Task.FromResult(Guid.NewGuid());
        }
    }

    private sealed class CountingReceiveHandler : ICommandHandler<SupplyOrderReceiveCommand>
    {
        public int Calls { get; private set; }

        public Task HandleAsync(SupplyOrderReceiveCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;

            return Task.CompletedTask;
        }
    }

    private sealed class MutableTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class InMemoryIdempotencyRecordRepository : IIdempotencyRecordRepository
    {
        private readonly Dictionary<(Guid, Guid), IdempotencyRecord> store = [];

        public Dictionary<(Guid, Guid), IdempotencyRecord>.ValueCollection All => store.Values;

        public Task<IdempotencyRecord?> FindAsync(Guid actorGuid, Guid key, CancellationToken cancellationToken = default)
            => Task.FromResult(store.GetValueOrDefault((actorGuid, key)));

        // The real repository saves with the unit of work; storing at once is enough for these tests, which
        // check what was recorded after each call.
        public void Add(IdempotencyRecord record) => store[(record.ActorGuid, record.Key)] = record;

        public Task<int> PurgeExpiredAsync(DateTimeOffset now, int batchSize, CancellationToken cancellationToken = default)
        {
            var expired = store.Where(r => r.Value.IsExpiredAt(now)).Take(batchSize).Select(r => r.Key).ToList();

            foreach (var key in expired)
            {
                store.Remove(key);
            }

            return Task.FromResult(expired.Count);
        }
    }
}
