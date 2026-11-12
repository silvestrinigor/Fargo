using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Core.Entities;

/// <summary>
/// Maps a concrete <see cref="Entity"/> CLR type to its <see cref="EntityType"/> when no instance
/// exists (e.g. when reporting that a looked-up entity was not found).
/// </summary>
public static class EntityTypeResolver
{
    public static EntityType Resolve<TEntity>()
        where TEntity : Entity
        => Resolve(typeof(TEntity));

    public static EntityType Resolve(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        if (typeof(Article).IsAssignableFrom(entityType))
        {
            return EntityType.Article;
        }

        if (typeof(Item).IsAssignableFrom(entityType))
        {
            return EntityType.Item;
        }

        if (typeof(Actor).IsAssignableFrom(entityType))
        {
            return EntityType.Actor;
        }

        if (typeof(UserGroup).IsAssignableFrom(entityType))
        {
            return EntityType.UserGroup;
        }

        if (typeof(Partition).IsAssignableFrom(entityType))
        {
            return EntityType.Partition;
        }

        throw new NotSupportedException(
            $"No {nameof(EntityType)} is mapped for entity type '{entityType.FullName}'.");
    }
}
