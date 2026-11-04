using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Core.Entities;

/// <summary>
/// Resolves the <see cref="EntityType"/> that corresponds to a concrete
/// <see cref="Entity"/> type at compile time.
/// </summary>
/// <remarks>
/// Every <see cref="Entity"/> instance carries its <see cref="Entity.EntityType"/>,
/// but that value is only available once an instance exists. This resolver
/// provides the same mapping from the CLR type alone, so callers that must name
/// an entity's type without an instance (for example when reporting that a
/// looked-up entity was not found) can do so without hardcoding the value at
/// each call site.
/// </remarks>
public static class EntityTypeResolver
{
    /// <summary>
    /// Gets the <see cref="EntityType"/> for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The concrete entity type to resolve.</typeparam>
    /// <returns>The <see cref="EntityType"/> that identifies <typeparamref name="TEntity"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <typeparamref name="TEntity"/> is not a mapped entity type.
    /// </exception>
    public static EntityType Resolve<TEntity>()
        where TEntity : Entity
        => Resolve(typeof(TEntity));

    /// <summary>
    /// Gets the <see cref="EntityType"/> for the specified entity CLR type.
    /// </summary>
    /// <param name="entityType">The concrete entity CLR type to resolve.</param>
    /// <returns>The <see cref="EntityType"/> that identifies <paramref name="entityType"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <paramref name="entityType"/> is not a mapped entity type.
    /// </exception>
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
