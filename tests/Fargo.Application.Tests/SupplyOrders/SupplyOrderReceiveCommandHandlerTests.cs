using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Events;
using Fargo.Core.Informations;
using NSubstitute;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Application.Tests.SupplyOrders;

public class SupplyOrderReceiveCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly ISupplyOrderRepository supplyOrderRepository = Substitute.For<ISupplyOrderRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IItemEventRepository itemEventRepository = Substitute.For<IItemEventRepository>();
    private readonly TestCurrentActor currentActor = new();

    private SupplyOrderReceiveCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            supplyOrderRepository,
            itemRepository,
            itemEventRepository,
            currentActor);

    private static Scalar Qty(double value) => value.Amount();

    private SupplyOrder PlacedOrder(Article article, Scalar ordered, Item? container = null)
    {
        var order = SupplyOrder.NewSupplyOrder(new Name("PO-1"));
        order.AddLine(article, ordered);
        if (container is not null)
        {
            order.SetReceivingContainer(container);
        }
        order.Place();
        supplyOrderRepository.GetByGuidAsync(order.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SupplyOrder?>(order));
        return order;
    }

    [Fact]
    public async Task HandleAsync_EventTracked_ShouldAppendCreationEvent()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ReceiveSupplyOrder]);
        var article = Article.NewArticle(new Name("Bulk sand"), ArticleTrackingMode.Event);
        var order = PlacedOrder(article, Qty(100));

        var command = new SupplyOrderReceiveCommand(order.Guid, new SupplyOrderReceiveDto(
            [new SupplyOrderReceiveLineDto(article.Guid, Qty(40.5))]));

        await CreateHandler().HandleAsync(command);

        itemEventRepository.Received(1).Add(Arg.Is<ItemEvent>(e =>
            e.ArticleGuid == article.Guid && e.ItemEventType == ItemEventType.Creation));
        itemRepository.DidNotReceive().Add(Arg.Any<Item>());
    }

    [Fact]
    public async Task HandleAsync_WhenFullyReceived_ShouldSetStatusReceived()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ReceiveSupplyOrder]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Event);
        var order = PlacedOrder(article, Qty(5));

        await CreateHandler().HandleAsync(new SupplyOrderReceiveCommand(order.Guid, new SupplyOrderReceiveDto(
            [new SupplyOrderReceiveLineDto(article.Guid, Qty(5))])));

        Assert.Equal(SupplyOrderStatus.Received, order.Status);
        Assert.True(order.Lines.Single().IsFullyReceived);
        Assert.False(order.Lines.Single().IsOverReceived);
    }

    [Fact]
    public async Task HandleAsync_WhenOverReceived_ShouldFlagOverReceiptAndStayReceived()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ReceiveSupplyOrder]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Event);
        var order = PlacedOrder(article, Qty(5));

        // Receive 5 (fully received), then 2 more (over-receipt) while already Received.
        await CreateHandler().HandleAsync(new SupplyOrderReceiveCommand(order.Guid, new SupplyOrderReceiveDto(
            [new SupplyOrderReceiveLineDto(article.Guid, Qty(5))])));
        await CreateHandler().HandleAsync(new SupplyOrderReceiveCommand(order.Guid, new SupplyOrderReceiveDto(
            [new SupplyOrderReceiveLineDto(article.Guid, Qty(2))])));

        Assert.Equal(SupplyOrderStatus.Received, order.Status);
        Assert.True(order.Lines.Single().IsOverReceived);
        Assert.Equal(7d, order.Lines.Single().ReceivedQuantity.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenArticleNotOnOrder_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.ReceiveSupplyOrder]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Event);
        var order = PlacedOrder(article, Qty(5));

        var command = new SupplyOrderReceiveCommand(order.Guid, new SupplyOrderReceiveDto(
            [new SupplyOrderReceiveLineDto(Guid.NewGuid(), Qty(1))]));

        await Assert.ThrowsAsync<FargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new SupplyOrderReceiveCommand(
                Guid.NewGuid(),
                new SupplyOrderReceiveDto([new SupplyOrderReceiveLineDto(Guid.NewGuid(), Qty(1))]))));
    }
}
