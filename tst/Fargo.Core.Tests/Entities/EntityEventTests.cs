using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;

namespace Fargo.Core.Tests.Entities;

public sealed class EntityEventTests
{
    [Fact]
    public void Constructor_Should_SetProperties()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityEvent = Event.NewEntityCreatedEvent<Article>(
            article,
            actorGuid,
            occurredAt);

        Assert.Equal(EntityType.Article, entityEvent.EntityType);
        Assert.Equal(EventType.Created, entityEvent.EventType);
        Assert.Equal(article.Guid, entityEvent.EntityGuid);
        Assert.Equal(actorGuid, entityEvent.ActorGuid);
        Assert.Equal(occurredAt, entityEvent.OccurredAt);
    }

    [Fact]
    public void Factory_Should_Throw_WhenRequiredArgumentsAreInvalid()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentNullException>(() => Event.NewEntityCreatedEvent<Article>(
            null!,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentException>(() => Event.NewEntityCreatedEvent<Article>(
            article,
            Guid.Empty,
            occurredAt));
    }

    [Fact]
    public void InsertedIntoPartition_Should_SetDetailsAndEventProperties()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var partition = Partition.CreatePartition(new Name("Partition"));
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityPartitionEvent = PartitionEvent.InsertedIntoPartition(
            article,
            partition,
            actorGuid,
            occurredAt);

        Assert.Equal(entityPartitionEvent.Event.Guid, entityPartitionEvent.Guid);
        Assert.Equal(EntityType.Article, entityPartitionEvent.EntityType);
        Assert.Equal(EventType.InsertedIntoPartition, entityPartitionEvent.Event.EventType);
        Assert.Equal(article.Guid, entityPartitionEvent.EntityGuid);
        Assert.Equal(partition.Guid, entityPartitionEvent.PartitionGuid);
        Assert.Equal(actorGuid, entityPartitionEvent.ActorGuid);
        Assert.Equal(occurredAt, entityPartitionEvent.OccurredAt);
    }

    [Fact]
    public void RemovedFromPartition_Should_SetDetailsAndEventProperties()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var partition = Partition.CreatePartition(new Name("Partition"));
        var actorGuid = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        var entityPartitionEvent = PartitionEvent.RemovedFromPartition(
            article,
            partition,
            actorGuid,
            occurredAt);

        Assert.Equal(entityPartitionEvent.Event.Guid, entityPartitionEvent.Guid);
        Assert.Equal(EntityType.Article, entityPartitionEvent.EntityType);
        Assert.Equal(EventType.RemovedFromPartition, entityPartitionEvent.Event.EventType);
        Assert.Equal(article.Guid, entityPartitionEvent.EntityGuid);
        Assert.Equal(partition.Guid, entityPartitionEvent.PartitionGuid);
        Assert.Equal(actorGuid, entityPartitionEvent.ActorGuid);
        Assert.Equal(occurredAt, entityPartitionEvent.OccurredAt);
    }

    [Fact]
    public void EntityPartitionEventFactories_Should_Throw_WhenRequiredArgumentsAreInvalid()
    {
        var article = Article.NewArticle(new Name("Article"), CreateDomainActor());
        var partition = Partition.CreatePartition(new Name("Partition"));
        var occurredAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentNullException>(() => PartitionEvent.InsertedIntoPartition<Article>(
            null!,
            partition,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentNullException>(() => PartitionEvent.InsertedIntoPartition(
            article,
            null!,
            Guid.NewGuid(),
            occurredAt));

        Assert.Throws<ArgumentException>(() => PartitionEvent.InsertedIntoPartition(
            article,
            partition,
            Guid.Empty,
            occurredAt));
    }
}
