using Fargo.Core.Articles;
using Fargo.Core.Partitions;

namespace Fargo.Core.Tests.Entities;

public sealed class EntityEventTests
{
    [Fact]
    public void Constructor_Should_SetProperties()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityEvent = EntityEvent.EntityCreated<Article>(
            article,
            actorGuid,
            occurredAt);

        Assert.Equal(EntityType.Article, entityEvent.EntityType);
        Assert.Equal(EntityEventType.Created, entityEvent.EventType);
        Assert.Equal(article.Guid, entityEvent.EntityGuid);
        Assert.Equal(actorGuid, entityEvent.ActorGuid);
        Assert.Equal(occurredAt, entityEvent.OccurredAt);
    }

    [Fact]
    public void Factory_Should_Throw_WhenRequiredArgumentsAreInvalid()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentNullException>(() => EntityEvent.EntityCreated<Article>(
            null!,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentException>(() => EntityEvent.EntityCreated<Article>(
            article,
            Guid.Empty,
            occurredAt));
    }

    [Fact]
    public void InsertedIntoPartition_Should_SetDetailsAndEventProperties()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var partition = new Partition(new Name("Partition"));
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityPartitionEvent = EntityPartitionEvent.InsertedIntoPartition(
            article,
            partition,
            actorGuid,
            occurredAt);

        Assert.Equal(entityPartitionEvent.Event.Guid, entityPartitionEvent.Guid);
        Assert.Equal(EntityType.Article, entityPartitionEvent.EntityType);
        Assert.Equal(EntityEventType.InsertedIntoPartition, entityPartitionEvent.Event.EventType);
        Assert.Equal(article.Guid, entityPartitionEvent.EntityGuid);
        Assert.Equal(partition.Guid, entityPartitionEvent.PartitionGuid);
        Assert.Equal(actorGuid, entityPartitionEvent.ActorGuid);
        Assert.Equal(occurredAt, entityPartitionEvent.OccurredAt);
    }

    [Fact]
    public void RemovedFromPartition_Should_SetDetailsAndEventProperties()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var partition = new Partition(new Name("Partition"));
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityPartitionEvent = EntityPartitionEvent.RemovedFromPartition(
            article,
            partition,
            actorGuid,
            occurredAt);

        Assert.Equal(entityPartitionEvent.Event.Guid, entityPartitionEvent.Guid);
        Assert.Equal(EntityType.Article, entityPartitionEvent.EntityType);
        Assert.Equal(EntityEventType.RemovedFromPartition, entityPartitionEvent.Event.EventType);
        Assert.Equal(article.Guid, entityPartitionEvent.EntityGuid);
        Assert.Equal(partition.Guid, entityPartitionEvent.PartitionGuid);
        Assert.Equal(actorGuid, entityPartitionEvent.ActorGuid);
        Assert.Equal(occurredAt, entityPartitionEvent.OccurredAt);
    }

    [Fact]
    public void EntityPartitionEventFactories_Should_Throw_WhenRequiredArgumentsAreInvalid()
    {
        var article = Article.CreateArticle(new Name("Article"));
        var partition = new Partition(new Name("Partition"));
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentNullException>(() => EntityPartitionEvent.InsertedIntoPartition<Article>(
            null!,
            partition,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentNullException>(() => EntityPartitionEvent.InsertedIntoPartition(
            article,
            null!,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentException>(() => EntityPartitionEvent.InsertedIntoPartition(
            article,
            partition,
            Guid.Empty,
            occurredAt));
    }
}
