using Fargo.Application.Authentication;
using Fargo.Application.Items;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;
using Fargo.Domain.System;
using Fargo.Domain.Users;
using NSubstitute;

namespace Fargo.Application.Tests.Items;

public sealed class ItemUpdateCommandHandlerTests
{
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();
    private readonly IPartitionRepository partitionRepository = Substitute.For<IPartitionRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    public ItemUpdateCommandHandlerTests()
    {
        currentUser.UserGuid.Returns(SystemService.SystemGuid);
    }

    [Fact]
    public async Task Handle_Should_MoveItemToParentContainer()
    {
        // Arrange
        var item = CreateItem();
        var parentContainer = CreateContainerItem();
        var sut = CreateHandler();

        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>()).Returns(item);
        itemRepository.GetByGuid(parentContainer.Guid, Arg.Any<CancellationToken>()).Returns(parentContainer);
        itemRepository.GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>()).Returns([]);

        var command = new ItemUpdateCommand(
            item.Guid,
            new ItemUpdateDto([], parentContainer.Guid));

        // Act
        await sut.Handle(command);

        // Assert
        Assert.Equal(parentContainer.Guid, item.ParentContainerGuid);
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveItemFromParentContainer_When_ParentContainerGuidIsNull()
    {
        // Arrange
        var item = CreateItem();
        var parentContainer = CreateContainerItem();
        var itemService = new ItemService(itemRepository);
        var sut = CreateHandler(itemService);

        itemRepository.GetByGuid(item.Guid, Arg.Any<CancellationToken>()).Returns(item);
        itemRepository.GetContainerDescendantGuids(item.Guid, false, Arg.Any<CancellationToken>()).Returns([]);

        await itemService.MoveToContainer(parentContainer, item);

        var command = new ItemUpdateCommand(
            item.Guid,
            new ItemUpdateDto([], null));

        // Act
        await sut.Handle(command);

        // Assert
        Assert.Null(item.ParentContainerGuid);
        await unitOfWork.Received(1).SaveChanges(Arg.Any<CancellationToken>());
    }

    private ItemUpdateCommandHandler CreateHandler(ItemService? itemService = null)
        => new(
            new ActorService(Substitute.For<IUserRepository>(), partitionRepository),
            itemRepository,
            partitionRepository,
            itemService ?? new ItemService(itemRepository),
            unitOfWork,
            currentUser);

    private static Item CreateItem()
        => new(CreateArticle());

    private static Item CreateContainerItem()
        => new(CreateContainerArticle());

    private static Article CreateArticle()
        => new()
        {
            Name = new Name("Test article")
        };

    private static Article CreateContainerArticle()
        => new(new ArticleContainer())
        {
            Name = new Name("Container article")
        };
}
