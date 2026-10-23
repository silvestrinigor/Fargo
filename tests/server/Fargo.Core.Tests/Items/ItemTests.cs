using Fargo.Core.Articles;
using Fargo.Core.Common;
using Fargo.Core.Informations;
using Fargo.Core.Items;
using Fargo.Core.Partitions;
using NSubstitute;

namespace Fargo.Core.Tests.Items;

public class ItemTests
{
    private static Article Container() => Article.NewArticleContainer(new Name("Box"));

    private static Article NonContainer() => Article.NewArticle(new Name("Widget"));

    [Fact]
    public void CreateItem_ShouldReferenceArticleAndGlobalPartition()
    {
        var article = NonContainer();

        var item = Item.CreateItem(article);

        Assert.Equal(article.Guid, item.ArticleGuid);
        Assert.Contains(FargoCoreWellKnowGuids.GlobalPartitionGuid, item.PartitionGuids);
        Assert.Null(item.ParentItemContainerGuid);
    }

    [Fact]
    public void PlaceInsideContainer_WhenValidContainer_ShouldSetParent()
    {
        var item = Item.CreateItem(NonContainer());
        var container = Item.CreateItem(Container());

        item.PlaceInsideContainer(container);

        Assert.Equal(container.Guid, item.ParentItemContainerGuid);
        Assert.Equal(container, item.ParentItemContainer);
    }

    [Fact]
    public void PlaceInsideContainer_WhenParentIsSelf_ShouldThrow()
    {
        var container = Item.CreateItem(Container());

        var ex = Assert.Throws<FargoCoreException>(() => container.PlaceInsideContainer(container));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void PlaceInsideContainer_WhenParentIsNotContainerArticle_ShouldThrow()
    {
        var item = Item.CreateItem(NonContainer());
        var nonContainer = Item.CreateItem(NonContainer());

        var ex = Assert.Throws<FargoCoreException>(() => item.PlaceInsideContainer(nonContainer));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void PlaceInsideContainer_WhenNull_ShouldThrow()
    {
        var item = Item.CreateItem(NonContainer());

        Assert.Throws<ArgumentNullException>(() => item.PlaceInsideContainer(null!));
    }

    [Fact]
    public void PlaceInsideContainer_WhenFixed_ShouldThrow()
    {
        var item = Item.CreateItem(NonContainer());
        var container = Item.CreateItem(Container());
        item.Fix();

        var ex = Assert.Throws<FargoCoreException>(() => item.PlaceInsideContainer(container));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void RemoveFromContainers_ShouldClearParent()
    {
        var item = Item.CreateItem(NonContainer());
        var container = Item.CreateItem(Container());
        item.PlaceInsideContainer(container);

        item.RemoveFromContainers();

        Assert.Null(item.ParentItemContainerGuid);
        Assert.Null(item.ParentItemContainer);
    }

    [Fact]
    public void RemoveFromContainers_WhenFixed_ShouldThrow()
    {
        var item = Item.CreateItem(NonContainer());
        item.Fix();

        var ex = Assert.Throws<FargoCoreException>(() => item.RemoveFromContainers());

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public void FixAndUnfix_ShouldToggleIsFixed()
    {
        var item = Item.CreateItem(NonContainer());

        item.Fix();
        Assert.True(item.IsFixed);

        item.Unfix();
        Assert.False(item.IsFixed);
    }

    [Fact]
    public void RemovePartition_WhenGlobal_ShouldThrow()
    {
        var item = Item.CreateItem(NonContainer());

        Assert.Throws<FargoCoreException>(
            () => item.RemovePartition(FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    [Fact]
    public void AddPartition_ShouldBeIdempotent()
    {
        var item = Item.CreateItem(NonContainer());
        var global = Partition.CreateGlobalPartition(default);
        var partition = Partition.CreatePartition(new Name("P"), global);

        item.AddPartition(partition);
        item.AddPartition(partition);

        Assert.Single(item.PartitionGuids, g => g == partition.Guid);
    }
}

public class ItemServiceTests
{
    private readonly IItemRepository itemRepository = Substitute.For<IItemRepository>();

    private ItemService Service() => new(itemRepository);

    [Fact]
    public async Task ValidateParentItemContainerHierarchyAssignmentAsync_WhenNoCycle_ShouldNotThrow()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Widget")));
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));

        itemRepository
            .GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        await Service().ValidateParentItemContainerHierarchyAssignmentAsync(container, item);
    }

    [Fact]
    public async Task ValidateParentItemContainerHierarchyAssignmentAsync_WhenParentIsSelf_ShouldThrow()
    {
        var container = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));

        itemRepository
            .GetContainedDescendantGuidsAsync(container.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));

        var ex = await Assert.ThrowsAsync<FargoCoreException>(
            () => Service().ValidateParentItemContainerHierarchyAssignmentAsync(container, container));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public async Task ValidateParentItemContainerHierarchyAssignmentAsync_WhenParentIsDescendant_ShouldThrow()
    {
        var item = Item.CreateItem(Article.NewArticleContainer(new Name("Box")));
        var candidateParent = Item.CreateItem(Article.NewArticleContainer(new Name("Inner")));

        // candidateParent is a descendant of item -> assigning it as item's parent is a cycle.
        itemRepository
            .GetContainedDescendantGuidsAsync(item.Guid, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyCollection<Guid>>([candidateParent.Guid]));

        var ex = await Assert.ThrowsAsync<FargoCoreException>(
            () => Service().ValidateParentItemContainerHierarchyAssignmentAsync(candidateParent, item));

        Assert.Equal(FargoErrorType.InvalidOperation, ex.ErrorType);
    }

    [Fact]
    public async Task ValidateParentItemContainerHierarchyAssignmentAsync_WhenNullArgs_ShouldThrow()
    {
        var item = Item.CreateItem(Article.NewArticle(new Name("Widget")));

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Service().ValidateParentItemContainerHierarchyAssignmentAsync(null!, item));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => Service().ValidateParentItemContainerHierarchyAssignmentAsync(item, null!));
    }
}
