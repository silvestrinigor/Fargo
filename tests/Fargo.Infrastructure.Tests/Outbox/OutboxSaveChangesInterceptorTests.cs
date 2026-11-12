using Fargo.Application.IntegrationEvents;
using Fargo.Core.Barcodes;
using Fargo.Core.Common;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using Fargo.Infrastructure.Outbox;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Fargo.Infrastructure.Tests.Outbox;

public class OutboxSaveChangesInterceptorTests
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    private readonly FailingSaveInterceptor failingSave = new();

    private FargoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FargoDbContext>()
            .UseInMemoryDatabase(databaseName)
            // The failing interceptor runs after the outbox interceptor, so it
            // simulates a save that fails once the messages are queued.
            .AddInterceptors(new OutboxSaveChangesInterceptor(TimeProvider.System), failingSave)
            .Options;

        return new FargoDbContext(options);
    }

    private async Task<List<OutboxMessage>> ReadOutboxAsync()
    {
        await using var context = CreateContext();

        return await context.OutboxMessages.OrderBy(m => m.Sequence).ToListAsync();
    }

    private async Task<Article> SaveNewArticleAsync(string name = "Widget")
    {
        await using var context = CreateContext();

        var article = Article.NewArticle(new Name(name));
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        return article;
    }

    [Fact]
    public async Task SaveChanges_WhenArticleAdded_ShouldQueueArticleCreated()
    {
        var article = await SaveNewArticleAsync();

        var message = Assert.Single(await ReadOutboxAsync());
        Assert.Equal(ArticleEventTypes.Created, message.EventType);
        Assert.Equal(article.Guid, message.AggregateGuid);
        Assert.Null(message.ProcessedAt);
    }

    [Fact]
    public async Task SaveChanges_WhenArticleAdded_ShouldStoreBodyWithTheArticleGuidAndPartitions()
    {
        var article = await SaveNewArticleAsync();

        var message = Assert.Single(await ReadOutboxAsync());
        using var body = JsonDocument.Parse(message.Body);

        Assert.Equal(
            ["eventId", "eventType", "occurredAt", "articleGuid", "partitionGuids"],
            body.RootElement.EnumerateObject().Select(p => p.Name));
        Assert.Equal(message.EventId, body.RootElement.GetProperty("eventId").GetGuid());
        Assert.Equal(ArticleEventTypes.Created, body.RootElement.GetProperty("eventType").GetString());
        Assert.Equal(article.Guid, body.RootElement.GetProperty("articleGuid").GetGuid());
        Assert.Equal(ArticleEventMessage.SchemaVersion, message.SchemaVersion);
    }

    [Fact]
    public async Task SaveChanges_WhenArticleAdded_ShouldReportItsGlobalPartition()
    {
        await SaveNewArticleAsync();

        var message = Assert.Single(await ReadOutboxAsync());

        Assert.Equal([FargoCoreWellKnowGuids.GlobalPartitionGuid], ReadPartitions(message));
    }

    [Fact]
    public async Task SaveChanges_WhenEditedWithoutLoadingPartitions_ShouldReportTheStoredPartitions()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            // Partitions are not included: they must come from the database.
            var loaded = await context.Articles.SingleAsync(a => a.Guid == article.Guid);
            loaded.Name = new Name("Renamed");
            await context.SaveChangesAsync();
        }

        var edited = (await ReadOutboxAsync()).Last();

        Assert.Equal(ArticleEventTypes.Edited, edited.EventType);
        Assert.Equal([FargoCoreWellKnowGuids.GlobalPartitionGuid], ReadPartitions(edited));
    }

    [Fact]
    public async Task SaveChanges_WhenPartitionAdded_ShouldReportTheOldAndNewPartitions()
    {
        var article = await SaveNewArticleAsync();
        var branch = await SaveBranchPartitionAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            loaded.AddPartition(await context.Partitions.SingleAsync(p => p.Guid == branch.Guid));
            await context.SaveChangesAsync();
        }

        var edited = (await ReadOutboxAsync()).Last();

        Assert.Equal(
            new[] { FargoCoreWellKnowGuids.GlobalPartitionGuid, branch.Guid }.Order(),
            ReadPartitions(edited));
    }

    [Fact]
    public async Task SaveChanges_WhenPartitionRemoved_ShouldStillReportTheRemovedPartition()
    {
        var article = await SaveNewArticleAsync();
        var branch = await SaveBranchPartitionAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            loaded.AddPartition(await context.Partitions.SingleAsync(p => p.Guid == branch.Guid));
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            loaded.RemovePartition(branch.Guid);
            await context.SaveChangesAsync();
        }

        var removal = (await ReadOutboxAsync()).Last();

        // Actors that could see the article only through the branch lose access
        // with this change, and must still be told about it.
        Assert.Equal(ArticleEventTypes.Edited, removal.EventType);
        Assert.Contains(branch.Guid, ReadPartitions(removal));
    }

    [Fact]
    public async Task SaveChanges_WhenArticleDeleted_ShouldReportThePartitionsItHad()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Barcode)
                .Include(a => a.Dimension)
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            context.Articles.Remove(loaded);
            await context.SaveChangesAsync();
        }

        var deleted = (await ReadOutboxAsync()).Last();

        Assert.Equal(ArticleEventTypes.Deleted, deleted.EventType);
        Assert.Equal([FargoCoreWellKnowGuids.GlobalPartitionGuid], ReadPartitions(deleted));
    }

    private async Task<Partition> SaveBranchPartitionAsync()
    {
        await using var context = CreateContext();

        var global = Partition.CreateGlobalPartition(new Name("Global"));
        var branch = Partition.CreatePartition(new Name("Branch"), global);
        context.Partitions.AddRange(global, branch);
        await context.SaveChangesAsync();

        return branch;
    }

    private static List<Guid> ReadPartitions(OutboxMessage message)
    {
        using var body = JsonDocument.Parse(message.Body);

        return [.. body.RootElement.GetProperty("partitionGuids").EnumerateArray().Select(p => p.GetGuid())];
    }

    [Fact]
    public async Task SaveChanges_WhenArticleCreatedWithItsParts_ShouldQueueOnlyArticleCreated()
    {
        await using (var context = CreateContext())
        {
            var source = Article.NewArticle(new Name("Source"));
            var variation = Article.NewArticleVariation(new Name("Variation"), source);
            source.Barcode.Ean13 = new Ean13("4006381333931");
            context.Articles.AddRange(source, variation);
            await context.SaveChangesAsync();
        }

        var messages = await ReadOutboxAsync();

        Assert.Equal(2, messages.Count);
        Assert.All(messages, m => Assert.Equal(ArticleEventTypes.Created, m.EventType));
    }

    [Fact]
    public async Task SaveChanges_WhenArticlePropertyEdited_ShouldQueueArticleEdited()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles.SingleAsync(a => a.Guid == article.Guid);
            loaded.Name = new Name("Renamed");
            await context.SaveChangesAsync();
        }

        var messages = await ReadOutboxAsync();

        Assert.Equal(
            [ArticleEventTypes.Created, ArticleEventTypes.Edited],
            messages.Select(m => m.EventType));
        Assert.Equal(article.Guid, messages[1].AggregateGuid);
    }

    [Fact]
    public async Task SaveChanges_WhenOnlyBarcodeEdited_ShouldQueueArticleEdited()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Barcode)
                .SingleAsync(a => a.Guid == article.Guid);
            loaded.Barcode.Ean13 = new Ean13("4006381333931");
            await context.SaveChangesAsync();
        }

        var edited = (await ReadOutboxAsync()).Last();
        Assert.Equal(ArticleEventTypes.Edited, edited.EventType);
        Assert.Equal(article.Guid, edited.AggregateGuid);
    }

    [Fact]
    public async Task SaveChanges_WhenOnlyPartitionAdded_ShouldQueueArticleEdited()
    {
        var article = await SaveNewArticleAsync();

        Partition branch;
        await using (var context = CreateContext())
        {
            var global = Partition.CreateGlobalPartition(new Name("Global"));
            branch = Partition.CreatePartition(new Name("Branch"), global);
            context.Partitions.AddRange(global, branch);
            await context.SaveChangesAsync();
        }

        // Saving partitions alone is not an article change.
        Assert.Single(await ReadOutboxAsync());

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            var loadedBranch = await context.Partitions.SingleAsync(p => p.Guid == branch.Guid);
            loaded.AddPartition(loadedBranch);
            await context.SaveChangesAsync();
        }

        var edited = (await ReadOutboxAsync()).Last();
        Assert.Equal(ArticleEventTypes.Edited, edited.EventType);
        Assert.Equal(article.Guid, edited.AggregateGuid);
    }

    [Fact]
    public async Task SaveChanges_WhenArticleAndPartsEditedTogether_ShouldQueueOneArticleEdited()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Barcode)
                .SingleAsync(a => a.Guid == article.Guid);
            loaded.Name = new Name("Renamed");
            loaded.Description = new Description("New description");
            loaded.Barcode.Ean13 = new Ean13("4006381333931");
            await context.SaveChangesAsync();
        }

        var messages = await ReadOutboxAsync();
        Assert.Equal(
            [ArticleEventTypes.Created, ArticleEventTypes.Edited],
            messages.Select(m => m.EventType));
    }

    [Fact]
    public async Task SaveChanges_WhenArticleDeleted_ShouldQueueOnlyArticleDeleted()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles
                .Include(a => a.Barcode)
                .Include(a => a.Dimension)
                .Include(a => a.Partitions)
                .SingleAsync(a => a.Guid == article.Guid);
            context.Articles.Remove(loaded);
            await context.SaveChangesAsync();
        }

        var messages = await ReadOutboxAsync();
        Assert.Equal(
            [ArticleEventTypes.Created, ArticleEventTypes.Deleted],
            messages.Select(m => m.EventType));
        Assert.Equal(article.Guid, messages[1].AggregateGuid);
    }

    [Fact]
    public async Task SaveChanges_WhenNothingChanged_ShouldQueueNothingMore()
    {
        var article = await SaveNewArticleAsync();

        await using (var context = CreateContext())
        {
            var loaded = await context.Articles.SingleAsync(a => a.Guid == article.Guid);
            loaded.Name = new Name("Widget");
            await context.SaveChangesAsync();
        }

        Assert.Single(await ReadOutboxAsync());
    }

    [Fact]
    public async Task SaveChanges_WhenSaveFails_ShouldPersistNoMessageAndNotDuplicateOnRetry()
    {
        await using var context = CreateContext();

        var article = Article.NewArticle(new Name("Widget"));
        context.Articles.Add(article);

        failingSave.FailNextSave = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        Assert.Empty(await ReadOutboxAsync());

        await context.SaveChangesAsync();

        var message = Assert.Single(await ReadOutboxAsync());
        Assert.Equal(ArticleEventTypes.Created, message.EventType);
        Assert.Equal(article.Guid, message.AggregateGuid);
    }

    [Fact]
    public async Task SaveChanges_Synchronous_ShouldAlsoQueueEvents()
    {
        await using (var context = CreateContext())
        {
            context.Articles.Add(Article.NewArticle(new Name("Widget")));
            context.SaveChanges();
        }

        Assert.Single(await ReadOutboxAsync());
    }

    private sealed class FailingSaveInterceptor : SaveChangesInterceptor
    {
        public bool FailNextSave { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (FailNextSave)
            {
                FailNextSave = false;

                throw new InvalidOperationException("Simulated save failure.");
            }

            return ValueTask.FromResult(result);
        }
    }
}
