using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Events;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Tests.Persistence;

/// <summary>
/// Item events are saved together with the item change, so the foreign keys and the cascading delete
/// are checked against PostgreSQL.
/// </summary>
public class ItemEventPersistenceTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var context = postgres.CreateContext();

        if (!await context.Partitions.AnyAsync(p => p.Guid == FargoCoreWellKnowGuids.GlobalPartitionGuid))
        {
            context.Partitions.Add(Partition.CreateGlobalPartition(new Name("global")));

            await context.SaveChangesAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<List<ItemEvent>> EventsOfAsync(FargoDbContext context, Guid itemGuid)
        => await context.ItemEvents
            .AsNoTracking()
            .Where(e => e.ItemGuid == itemGuid)
            .OrderBy(e => e.OccuredAt)
            .ToListAsync();

    private static (Article Widget, Article Box) NewArticles(FargoDbContext context)
    {
        var widget = Article.NewArticle(new Name($"Widget {Guid.NewGuid()}"));
        var box = Article.NewArticleContainer(new Name($"Box {Guid.NewGuid()}"));

        context.Articles.AddRange(widget, box);

        return (widget, box);
    }

    [Fact]
    public async Task SaveChanges_ShouldKeepTheEventsOfAnItemThroughCreationMovesAndDeletion()
    {
        await using var context = postgres.CreateContext();
        var (widget, box) = NewArticles(context);
        var first = Item.CreateItem(box);
        var second = Item.CreateItem(box);
        var item = Item.CreateItem(widget);
        item.PlaceInsideContainer(first);
        context.Items.AddRange(first, second, item);
        context.ItemEvents.Add(ItemEvent.NewItemCreatedEvent(item));
        await context.SaveChangesAsync();

        var previousParentGuid = item.ParentItemContainerGuid;
        item.PlaceInsideContainer(second);
        context.ItemEvents.Add(ItemEvent.NewItemMovedEvent(item, previousParentGuid));
        await context.SaveChangesAsync();

        context.ItemEvents.Add(ItemEvent.NewItemDeletedEvent(item));
        context.Items.Remove(item);
        await context.SaveChangesAsync();

        Assert.False(await context.Items.AnyAsync(i => i.Guid == item.Guid));

        var events = await EventsOfAsync(context, item.Guid);

        Assert.Equal(
            [ItemEventType.Creation, ItemEventType.Moviment, ItemEventType.Deletion],
            events.Select(e => e.ItemEventType));
        Assert.Equal(first.Guid, events[0].ToItemContainerGuid);
        Assert.Equal(first.Guid, events[1].FromItemContainerGuid);
        Assert.Equal(second.Guid, events[1].ToItemContainerGuid);
        Assert.Equal(second.Guid, events[2].FromItemContainerGuid);
        Assert.All(events, e => Assert.Equal(widget.Guid, e.ArticleGuid));
    }

    [Fact]
    public async Task SaveChanges_WhenAContainerIsDeletedWithItsItems_ShouldKeepTheirDeletionEvents()
    {
        await using var context = postgres.CreateContext();
        var (widget, box) = NewArticles(context);
        var outer = Item.CreateItem(box);
        var inner = Item.CreateItem(box);
        inner.PlaceInsideContainer(outer);
        var item = Item.CreateItem(widget);
        item.PlaceInsideContainer(inner);
        context.Items.AddRange(outer, inner, item);
        await context.SaveChangesAsync();

        var repository = new ItemRepository(context);
        var contained = await repository.GetContainedItemsAsync(outer.Guid);

        Assert.Equivalent(
            new[] { new ContainedItem(inner.Guid, box.Guid), new ContainedItem(item.Guid, widget.Guid) },
            contained);

        // Only the root is tracked: the items inside it go away through the database cascade.
        context.ChangeTracker.Clear();
        var trackedOuter = await repository.GetByGuidAsync(outer.Guid);

        foreach (var containedItem in contained)
        {
            context.ItemEvents.Add(ItemEvent.NewContainedItemDeletedEvent(containedItem.ItemGuid, containedItem.ArticleGuid));
        }

        context.ItemEvents.Add(ItemEvent.NewItemDeletedEvent(trackedOuter!));
        context.Items.Remove(trackedOuter!);
        await context.SaveChangesAsync();

        Assert.False(await context.Items.AnyAsync(i => i.Guid == item.Guid));

        foreach (var deleted in new[] { outer, inner, item })
        {
            var deletion = Assert.Single(await EventsOfAsync(context, deleted.Guid));

            Assert.Equal(ItemEventType.Deletion, deletion.ItemEventType);
            Assert.Null(deletion.FromItemContainerGuid);
        }

        // The deleted container had no parent, and the items inside it only have the flag.
        Assert.True((await EventsOfAsync(context, outer.Guid)).Single().FromNoContainer);
        Assert.False((await EventsOfAsync(context, item.Guid)).Single().FromNoContainer);
    }

    [Fact]
    public async Task GetItemMoviments_ShouldListTheCreationInsideAContainerAndEveryMovement()
    {
        await using var context = postgres.CreateContext();
        var (widget, box) = NewArticles(context);
        var first = Item.CreateItem(box);
        var second = Item.CreateItem(box);
        var item = Item.CreateItem(widget);
        item.PlaceInsideContainer(first);
        context.Items.AddRange(first, second, item);
        context.ItemEvents.Add(ItemEvent.NewItemCreatedEvent(item));
        await context.SaveChangesAsync();

        var previousParentGuid = item.ParentItemContainerGuid;
        item.PlaceInsideContainer(second);
        context.ItemEvents.Add(ItemEvent.NewItemMovedEvent(item, previousParentGuid));
        await context.SaveChangesAsync();

        var moviments = await new ItemRepository(context)
            .GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(item.Guid);

        Assert.NotNull(moviments);
        Assert.Equal(
            [first.Guid, second.Guid],
            moviments.Select(m => m.MovedToItemContainerGuid));
    }
}