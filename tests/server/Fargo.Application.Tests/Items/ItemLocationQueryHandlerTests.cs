using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemLocationQueryHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemQueryRepository itemRepository = Substitute.For<IItemQueryRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemLocationQueryHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemRepository,
            currentActor,
            Substitute.For<ILogger<ItemLocationQueryHandler>>());

    private static ItemDto SampleItem()
        => new(Guid.NewGuid(), Guid.NewGuid(), null, false, []);

    [Fact]
    public async Task HandleAsync_WhenAtIsNull_ShouldQueryCurrentLocationAndReturnChain()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);
        var itemGuid = Guid.NewGuid();
        IReadOnlyCollection<ItemDto> chain = [SampleItem(), SampleItem()];
        itemRepository
            .GetLocationInfoByGuidOrderedByDepthAsync(itemGuid, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(chain));

        var result = await CreateHandler().HandleAsync(new ItemLocationQuery(itemGuid));

        Assert.Same(chain, result);
        await itemRepository.Received(1)
            .GetLocationInfoByGuidOrderedByDepthAsync(itemGuid, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>());
        await itemRepository.DidNotReceive()
            .GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
                Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAtIsProvided_ShouldQueryHistoricalLocationAndReturnChain()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);
        var itemGuid = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow.AddDays(-1);
        IReadOnlyCollection<ItemDto> chain = [SampleItem()];
        itemRepository
            .GetLocationInfoByGuidAtInstantOrderedByDepthAsync(itemGuid, at, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(chain));

        var result = await CreateHandler().HandleAsync(new ItemLocationQuery(itemGuid, at));

        Assert.Same(chain, result);
        await itemRepository.Received(1)
            .GetLocationInfoByGuidAtInstantOrderedByDepthAsync(itemGuid, at, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>());
        await itemRepository.DidNotReceive()
            .GetLocationInfoByGuidOrderedByDepthAsync(Arg.Any<Guid>(), Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(new ItemLocationQuery(Guid.NewGuid())));
    }

    [Fact]
    public async Task HandleAsync_WhenNoLocationFound_ShouldReturnEmptyCollection()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid);
        var itemGuid = Guid.NewGuid();
        IReadOnlyCollection<ItemDto> empty = [];
        itemRepository
            .GetLocationInfoByGuidOrderedByDepthAsync(itemGuid, Arg.Any<IReadOnlyCollection<Guid>?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(empty));

        var result = await CreateHandler().HandleAsync(new ItemLocationQuery(itemGuid));

        Assert.Empty(result);
    }
}
