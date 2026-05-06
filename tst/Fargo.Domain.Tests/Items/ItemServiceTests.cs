using Fargo.Domain.Articles;
using Fargo.Domain.Items;
using NSubstitute;

namespace Fargo.Domain.Tests.Items;

public sealed class ItemServiceTests
{
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();

    [Fact]
    public async Task MoveToContainer_Should_SetParentContainer()
    {
        // Arrange
        var parent = CreateContainerItem();
        var member = CreateItem();
        var sut = new ItemService(itemRepository);

        itemRepository
            .GetContainerDescendantGuids(member.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await sut.MoveToContainer(parent, member);

        // Assert
        Assert.Equal(parent.Guid, member.ParentContainerGuid);
        Assert.Equal(parent.Guid, member.ParentContainer?.Item.Guid);
    }

    [Fact]
    public async Task MoveToContainer_Should_Throw_When_ItemIsOwnContainer()
    {
        // Arrange
        var item = CreateContainerItem();
        var sut = new ItemService(itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<ItemCannotBeOwnContainerFargoDomainException>(
            () => sut.MoveToContainer(item, item));

        // Assert
        Assert.Equal(item.Guid, exception.ItemGuid);
    }

    [Fact]
    public async Task MoveToContainer_Should_Throw_When_ParentItemIsNotContainer()
    {
        // Arrange
        var parent = CreateItem();
        var member = CreateItem();
        var sut = new ItemService(itemRepository);

        // Act
        var exception = await Assert.ThrowsAsync<ItemParentIsNotContainerFargoDomainException>(
            () => sut.MoveToContainer(parent, member));

        // Assert
        Assert.Equal(parent.Guid, exception.ParentItemGuid);
    }

    [Fact]
    public async Task MoveToContainer_Should_Throw_When_MoveCreatesCircularHierarchy()
    {
        // Arrange
        var parent = CreateContainerItem();
        var member = CreateItem();
        var sut = new ItemService(itemRepository);

        itemRepository
            .GetContainerDescendantGuids(member.Guid, false, Arg.Any<CancellationToken>())
            .Returns([parent.Guid]);

        // Act
        var exception = await Assert.ThrowsAsync<ItemCircularContainerHierarchyFargoDomainException>(
            () => sut.MoveToContainer(parent, member));

        // Assert
        Assert.Equal(parent.Guid, exception.ParentContainerItemGuid);
        Assert.Equal(member.Guid, exception.MemberItemGuid);
    }

    [Fact]
    public async Task RemoveFromContainer_Should_ClearParentContainer()
    {
        // Arrange
        var parent = CreateContainerItem();
        var member = CreateItem();
        var sut = new ItemService(itemRepository);

        itemRepository
            .GetContainerDescendantGuids(member.Guid, false, Arg.Any<CancellationToken>())
            .Returns([]);

        await sut.MoveToContainer(parent, member);

        // Act
        ItemService.RemoveFromContainer(member);

        // Assert
        Assert.Null(member.ParentContainerGuid);
        Assert.Null(member.ParentContainer);
    }

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
