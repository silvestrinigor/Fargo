using Fargo.Core;
using Fargo.Core.Articles;

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
}
