using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Domain.Events;
using Fargo.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace Fargo.Infrastructure.Events;

/// <summary>
/// Decorator over <see cref="IFargoEventPublisher"/> that persists a domain <see cref="Event"/>
/// record to the database before delegating each publish call to the inner publisher.
/// Persistence failures are logged and do not block the inner publisher.
/// </summary>
public sealed class EventPersistingPublisher(
    IFargoEventPublisher inner,
    FargoDbContext db,
    ICurrentUser currentUser,
    ILogger<EventPersistingPublisher> logger
) : IFargoEventPublisher
{
    public async Task PublishArticleCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
    {
        await Save(EventType.ArticleCreated, EntityType.Article, guid, ct);
        await inner.PublishArticleCreated(guid, partitionGuids, ct);
    }

    public async Task PublishArticleUpdated(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.ArticleUpdated, EntityType.Article, guid, ct);
        await inner.PublishArticleUpdated(guid, ct);
    }

    public async Task PublishArticleDeleted(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.ArticleDeleted, EntityType.Article, guid, ct);
        await inner.PublishArticleDeleted(guid, ct);
    }

    public async Task PublishItemCreated(Guid guid, Guid articleGuid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
    {
        await Save(EventType.ItemCreated, EntityType.Item, guid, ct);
        await inner.PublishItemCreated(guid, articleGuid, partitionGuids, ct);
    }

    public async Task PublishItemUpdated(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.ItemUpdated, EntityType.Item, guid, ct);
        await inner.PublishItemUpdated(guid, ct);
    }

    public async Task PublishItemDeleted(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.ItemDeleted, EntityType.Item, guid, ct);
        await inner.PublishItemDeleted(guid, ct);
    }

    public async Task PublishUserCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
    {
        await Save(EventType.UserCreated, EntityType.User, guid, ct);
        await inner.PublishUserCreated(guid, nameid, partitionGuids, ct);
    }

    public async Task PublishUserUpdated(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.UserUpdated, EntityType.User, guid, ct);
        await inner.PublishUserUpdated(guid, ct);
    }

    public async Task PublishUserDeleted(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.UserDeleted, EntityType.User, guid, ct);
        await inner.PublishUserDeleted(guid, ct);
    }

    public async Task PublishUserGroupCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
    {
        await Save(EventType.UserGroupCreated, EntityType.UserGroup, guid, ct);
        await inner.PublishUserGroupCreated(guid, nameid, partitionGuids, ct);
    }

    public async Task PublishUserGroupUpdated(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.UserGroupUpdated, EntityType.UserGroup, guid, ct);
        await inner.PublishUserGroupUpdated(guid, ct);
    }

    public async Task PublishUserGroupDeleted(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.UserGroupDeleted, EntityType.UserGroup, guid, ct);
        await inner.PublishUserGroupDeleted(guid, ct);
    }

    public async Task PublishPartitionCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
    {
        await Save(EventType.PartitionCreated, EntityType.Partition, guid, ct);
        await inner.PublishPartitionCreated(guid, partitionGuids, ct);
    }

    public async Task PublishPartitionUpdated(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.PartitionUpdated, EntityType.Partition, guid, ct);
        await inner.PublishPartitionUpdated(guid, ct);
    }

    public async Task PublishPartitionDeleted(Guid guid, CancellationToken ct = default)
    {
        await Save(EventType.PartitionDeleted, EntityType.Partition, guid, ct);
        await inner.PublishPartitionDeleted(guid, ct);
    }

    private async Task Save(EventType eventType, EntityType entityType, Guid entityGuid, CancellationToken ct)
    {
        try
        {
            db.Events.Add(new Event
            {
                EventType = eventType,
                EntityType = entityType,
                EntityGuid = entityGuid,
                ActorGuid = currentUser.UserGuid,
            });

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist event {EventType} for entity {EntityGuid}.", eventType, entityGuid);
        }
    }
}
