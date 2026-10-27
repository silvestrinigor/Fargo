using Fargo.Application.Common;
using Fargo.Application.Items;
using Fargo.Application.Tests.Common;
using Fargo.Core.Actors;
using Fargo.Core.Items.Events;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public class ItemEventsQueryHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IItemEventQueryRepository itemEventQueryRepository = Substitute.For<IItemEventQueryRepository>();
    private readonly TestCurrentActor currentActor = new();

    private ItemEventsQueryHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            itemEventQueryRepository,
            currentActor,
            Substitute.For<ILogger<ItemEventsQueryHandler>>());

    private static PagedResult<ItemEventDto> EmptyPage()
        => new([], 0, Page.FirstPage.Value, Limit.MaxLimit.Value);

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrowAndNotQuery()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        var query = new ItemEventsQuery(new Pagination());

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(query));

        await itemEventQueryRepository.DidNotReceive().GetManyInfoOrderedByOccuredAtAsync(
            Arg.Any<Pagination>(),
            Arg.Any<IReadOnlyCollection<Guid>?>(),
            Arg.Any<Guid?>(),
            Arg.Any<ItemEventType?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPermitted_ShouldForwardFiltersAndActorPartitionScope()
    {
        var partition = Guid.NewGuid();
        ActorTestFactory.ConfigureUserActor(
            userRepository,
            currentActor.Guid,
            permissions: [ActionType.ReadItemEvents],
            partitionAccesses: [partition]);

        var articleGuid = Guid.NewGuid();
        var periodStart = DateTimeOffset.UtcNow.AddDays(-7);
        var periodEnd = DateTimeOffset.UtcNow;
        var pagination = new Pagination(new Page(2), new Limit(10));

        itemEventQueryRepository.GetManyInfoOrderedByOccuredAtAsync(
                Arg.Any<Pagination>(),
                Arg.Any<IReadOnlyCollection<Guid>?>(),
                Arg.Any<Guid?>(),
                Arg.Any<ItemEventType?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(EmptyPage()));

        var query = new ItemEventsQuery(
            pagination,
            ArticleGuid: articleGuid,
            EventType: ItemEventType.Moviment,
            PeriodStart: periodStart,
            PeriodEnd: periodEnd);

        var result = await CreateHandler().HandleAsync(query);

        Assert.NotNull(result);

        await itemEventQueryRepository.Received(1).GetManyInfoOrderedByOccuredAtAsync(
            pagination,
            Arg.Is<IReadOnlyCollection<Guid>?>(p => p != null && p.Contains(partition)),
            articleGuid,
            ItemEventType.Moviment,
            periodStart,
            periodEnd,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenActorNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureMissingActor(userRepository, currentActor.Guid);

        var query = new ItemEventsQuery(new Pagination());

        await Assert.ThrowsAsync<ActorNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(query));
    }
}
