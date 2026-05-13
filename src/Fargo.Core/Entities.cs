namespace Fargo.Core;

/// <summary>
/// Defines the contract for domain entities identified by a <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// An entity is uniquely identified by its <see cref="Guid"/> and uses
/// identity-based equality semantics.
///
/// Two entities are considered equal when they are of the same concrete type
/// and have the same non-empty identifier.
/// </remarks>
public interface IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <remarks>
    /// The identifier uniquely distinguishes the entity within the domain.
    /// It must not be <see cref="Guid.Empty"/>.
    /// </remarks>
    Guid Guid { get; }
}

/// <summary>
/// Base class for domain entities identified by a <see cref="Guid"/>.
///
/// Implements identity-based equality comparison, meaning two entities
/// are considered equal when:
/// - They are of the same concrete type
/// - Their Guid identifiers are equal
/// - The identifier is not <see cref="Guid.Empty"/>
///
/// This class also overloads equality operators (== and !=)
/// to provide value semantics based on identity.
/// </summary>
public abstract class Entity : IEntity
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    /// <remarks>
    /// A new identifier is generated automatically using <see cref="Guid.NewGuid"/>
    /// when the entity instance is created.
    ///
    /// The identifier cannot be <see cref="Guid.Empty"/>. Attempting to initialize
    /// this property with an empty value results in an <see cref="ArgumentException"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the assigned value is <see cref="Guid.Empty"/>.
    /// </exception>
    public Guid Guid
    {
        get;
        init
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Entity Guid cannot be empty.", nameof(value));
            }

            field = value;
        }
    } = Guid.NewGuid();

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    ///
    /// Equality is based on:
    /// - Same concrete type
    /// - Same non-empty Guid value
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>
    /// True if both entities have the same type and Guid; otherwise, false.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (Guid == Guid.Empty || other.Guid == Guid.Empty)
        {
            return false;
        }

        return Guid == other.Guid;
    }

    /// <summary>
    /// Returns a hash code for this entity based on its Guid.
    /// </summary>
    /// <returns>A hash code derived from the Guid identifier.</returns>
    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entities are equal based on identity.
    /// </summary>
    /// <param name="a">The first entity.</param>
    /// <param name="b">The second entity.</param>
    /// <returns>True if both entities are equal; otherwise, false.</returns>
    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two entities are not equal based on identity.
    /// </summary>
    /// <param name="a">The first entity.</param>
    /// <param name="b">The second entity.</param>
    /// <returns>True if the entities are not equal; otherwise, false.</returns>
    public static bool operator !=(Entity? a, Entity? b)
        => !(a == b);
}
/// <summary>
/// Represents a base entity that tracks last modification metadata.
/// </summary>
/// <remarks>
/// This entity extends <see cref="Entity"/> and implements
/// <see cref="IModifiedEntity"/> to provide information about
/// the last modification performed on the entity.
///
/// The modification metadata is expected to be managed automatically
/// by the application or infrastructure layer (for example, the EF Core
/// change tracker).
///
/// <para>
/// <strong>Important:</strong>
/// <see cref="EditedByGuid"/> is expected to be <see langword="null"/>
/// only during the entity creation phase. After the entity is tracked
/// and persisted, this value should normally be populated.
/// A <see langword="null"/> value outside this scenario typically
/// indicates a misconfiguration or a missing auditing operation.
/// </para>
/// </remarks>
public abstract class ModifiedEntity : Entity, IModifiedEntity
{
    /// <inheritdoc />
    public Guid? EditedByGuid
    {
        get;
        private set;
    }

    /// <inheritdoc />
    public void MarkAsEdited(Guid actorGuid)
    {
        EditedByGuid = actorGuid;
    }
}
/// <summary>
/// Defines the contract for entities that track last modification metadata.
/// </summary>
/// <remarks>
/// Implementations of this interface expose information about the last
/// modification performed on the entity, including the actor responsible
/// for the change.
///
/// The auditing values are typically assigned by the application or
/// infrastructure layer during persistence operations.
/// </remarks>
public interface IModifiedEntity
{
    /// <summary>
    /// Gets the unique identifier of the actor that last modified the entity.
    /// </summary>
    /// <remarks>
    /// This value is <see langword="null"/> when the entity has not
    /// been modified since its creation.
    ///
    /// When the modification is performed by an internal system process,
    /// implementations should use the reserved audit-origin guid used by
    /// the infrastructure layer.
    /// </remarks>
    Guid? EditedByGuid { get; }

    /// <summary>
    /// Marks the entity as edited by the specified actor.
    /// </summary>
    /// <param name="actorGuid">
    /// The unique identifier of the actor performing the modification.
    /// </param>
    /// <remarks>
    /// This method updates the modification audit metadata of the entity.
    /// Implementations are expected to set the identifier of the actor
    /// responsible for the change and any related modification metadata
    /// (such as timestamps, if applicable).
    ///
    /// When the modification is performed by the system, the caller should pass
    /// the reserved system audit guid from the infrastructure layer.
    /// </remarks>
    void MarkAsEdited(Guid actorGuid);
}
/// <summary>
/// Defines a contract for entities that belong to an audited aggregate
/// and whose modifications must update the audit metadata of the aggregate root.
/// </summary>
/// <remarks>
/// Implementations of this interface indicate that any change to the current
/// entity should propagate a modification update to a parent entity that
/// implements <see cref="IModifiedEntity"/>.
///
/// This is typically used for aggregate members whose lifecycle is controlled
/// by an audited aggregate root, ensuring that changes within the aggregate
/// are consistently reflected in the root's modification metadata.
/// </remarks>
public interface IModifiedEntityMember
{
    /// <summary>
    /// Gets the parent entity whose modification metadata must be updated
    /// when this entity changes.
    /// </summary>
    /// <remarks>
    /// The returned entity must implement <see cref="IModifiedEntity"/> and is
    /// expected to have its audit state updated (e.g., via
    /// <see cref="IModifiedEntity.MarkAsEdited(Guid)"/>) whenever this member
    /// is modified.
    /// </remarks>
    IModifiedEntity ParentEditedEntity { get; }
}

/// <summary>
/// Represents an entity that can be activated or deactivated.
/// </summary>
/// <remarks>
/// Implementing types should define how activation state is managed,
/// typically controlling availability or lifecycle behavior
/// within the domain.
/// </remarks>
public interface IActivable
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Activates the entity.
    /// </summary>
    /// <remarks>
    /// Implementations should set <see cref="IsActive"/> to <c>true</c>
    /// and apply any domain rules associated with activation.
    /// </remarks>
    void Activate();

    /// <summary>
    /// Deactivates the entity.
    /// </summary>
    /// <remarks>
    /// Implementations should set <see cref="IsActive"/> to <c>false</c>
    /// and enforce any domain rules associated with deactivation.
    /// </remarks>
    void Deactivate();
}
