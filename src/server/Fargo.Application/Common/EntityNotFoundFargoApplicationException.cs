using Fargo.Core.Common;
using Fargo.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common;

public class EntityNotFoundFargoApplicationException : FargoApplicationException
{
    public Guid EntityGuid { get; init; }

    public EntityType EntityType { get; init; }

    public EntityNotFoundFargoApplicationException(Guid entityGuid, EntityType entityType)
        : base($"Entity '{entityGuid}' of type '{entityType}' was not found.", FargoErrorType.EntityNotFound)
    {
        EntityGuid = entityGuid;

        EntityType = entityType;
    }

    /// <summary>
    /// Throws when <paramref name="entity"/> is null. The entity type reported by
    /// the exception is resolved from <typeparamref name="TEntity"/>, so callers
    /// do not hardcode it at the call site.
    /// </summary>
    /// <typeparam name="TEntity">The concrete entity type that was looked up.</typeparam>
    /// <param name="entity">The looked-up entity, or <see langword="null"/> when not found.</param>
    /// <param name="entityGuid">The identifier that was looked up.</param>
    public static void ThrowIfNull<TEntity>([NotNull] TEntity? entity, Guid entityGuid)
        where TEntity : Entity
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoApplicationException(entityGuid, EntityTypeResolver.Resolve<TEntity>());
        }
    }

    /// <summary>
    /// Throws when <paramref name="entity"/> is null. Overload for objects that
    /// are not themselves an <see cref="Entity"/> (for example the <c>User</c> and
    /// <c>ServiceAccount</c> satellites, whose identity lives on their owning
    /// actor), where the entity type must be supplied explicitly.
    /// </summary>
    public static void ThrowIfNull<T>([NotNull] T? entity, Guid entityGuid, EntityType type)
        where T : class
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoApplicationException(entityGuid, type);
        }
    }
}
