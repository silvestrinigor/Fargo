using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Application.Tests.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.SupplyOrders;
using Fargo.Core.Informations;
using NSubstitute;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Application.Tests.SupplyOrders;

public class SupplyOrderCreateCommandHandlerTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly ISupplyOrderRepository supplyOrderRepository = Substitute.For<ISupplyOrderRepository>();
    private readonly IArticleRepository articleRepository = Substitute.For<IArticleRepository>();
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly TestCurrentActor currentActor = new();

    private SupplyOrderCreateCommandHandler CreateHandler()
        => new(
            ActorTestFactory.Resolver(userRepository),
            supplyOrderRepository,
            articleRepository,
            itemRepository,
            partitionRepository,
            currentActor);

    private static Scalar Qty(double value) => value.Amount();

    [Fact]
    public async Task HandleAsync_WhenValidWithLines_ShouldAddOrderWithLines()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateSupplyOrder]);
        var article = Article.NewArticle(new Name("Widget"), ArticleTrackingMode.Event);
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var command = new SupplyOrderCreateCommand(new SupplyOrderCreateDto(
            new Name("PO-1"),
            Lines: [new SupplyOrderLineCreateDto(article.Guid, Qty(10))]));

        var result = await CreateHandler().HandleAsync(command);

        supplyOrderRepository.Received(1).Add(Arg.Is<SupplyOrder>(o =>
            o.Guid == result
            && o.Lines.Count == 1
            && o.Lines.Single().ArticleGuid == article.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenLineArticleIsEntityTracked_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateSupplyOrder]);
        var article = Article.NewArticle(new Name("Widget"));
        articleRepository.GetByGuidAsync(article.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(article));

        var command = new SupplyOrderCreateCommand(new SupplyOrderCreateDto(
            new Name("PO-1"),
            Lines: [new SupplyOrderLineCreateDto(article.Guid, Qty(10))]));

        await Assert.ThrowsAsync<Fargo.Core.Common.FargoCoreException>(
            () => CreateHandler().HandleAsync(command));

        supplyOrderRepository.DidNotReceive().Add(Arg.Any<SupplyOrder>());
    }

    [Fact]
    public async Task HandleAsync_WhenReceivingContainerProvided_ShouldSetReceivingContainer()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateSupplyOrder]);
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Dock A")));
        itemRepository.GetByGuidAsync(container.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(container));

        var command = new SupplyOrderCreateCommand(new SupplyOrderCreateDto(
            new Name("PO-1"),
            ReceivingContainerGuid: container.Guid));

        await CreateHandler().HandleAsync(command);

        supplyOrderRepository.Received(1).Add(Arg.Is<SupplyOrder>(o =>
            o.ReceivingContainerGuid == container.Guid));
    }

    [Fact]
    public async Task HandleAsync_WhenReceivingContainerIsNotAContainer_ShouldThrowDomainException()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateSupplyOrder]);
        var notAContainer = Item.CreateItem(Article.NewArticle(new Name("Widget")));
        itemRepository.GetByGuidAsync(notAContainer.Guid, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Item?>(notAContainer));

        var command = new SupplyOrderCreateCommand(new SupplyOrderCreateDto(
            new Name("PO-1"),
            ReceivingContainerGuid: notAContainer.Guid));

        await Assert.ThrowsAsync<Fargo.Core.Common.FargoCoreException>(
            () => CreateHandler().HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionDenied_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, permissions: []);

        await Assert.ThrowsAsync<ActorPermissionDeniedFargoApplicationException>(
            () => CreateHandler().HandleAsync(new SupplyOrderCreateCommand(new SupplyOrderCreateDto(new Name("PO-1")))));
    }

    [Fact]
    public async Task HandleAsync_WhenLineArticleNotFound_ShouldThrow()
    {
        ActorTestFactory.ConfigureUserActor(userRepository, currentActor.Guid, [ActionType.CreateSupplyOrder]);
        var missing = Guid.NewGuid();
        articleRepository.GetByGuidAsync(missing, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Article?>(null));

        var command = new SupplyOrderCreateCommand(new SupplyOrderCreateDto(
            new Name("PO-1"),
            Lines: [new SupplyOrderLineCreateDto(missing, Qty(5))]));

        await Assert.ThrowsAsync<EntityNotFoundFargoApplicationException>(
            () => CreateHandler().HandleAsync(command));
    }
}
