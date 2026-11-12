using Fargo.Core.Common;
using Fargo.Core.Entities;
using System.Diagnostics.CodeAnalysis;

namespace Fargo.Application.Common.Exceptions;

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

    public static void ThrowIfNull<TEntity>([NotNull] TEntity? entity, Guid entityGuid)
        where TEntity : Entity
    {
        if (entity is null)
        {
            throw new EntityNotFoundFargoApplicationException(entityGuid, EntityTypeResolver.Resolve<TEntity>());
        }
    }

    /// <summary>
    /// For objects that are not an <see cref="Entity"/> themselves, such as the User and
    /// ServiceAccount satellites, whose identity lives on their owning actor.
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
