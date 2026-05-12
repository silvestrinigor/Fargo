using Fargo.Core.Partitions;
using Fargo.Core.System;
using Fargo.Core.Users;

namespace Fargo.Core;

#region Exceptions

/// <summary>
/// Base exception for all domain-related errors in the Fargo domain model.
///
/// Domain exceptions represent violations of business rules and should
/// be thrown only from the domain layer.
/// </summary>
public abstract class FargoDomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class.
    /// </summary>
    protected FargoDomainException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    protected FargoDomainException(string? message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FargoDomainException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message describing the error.</param>
    /// <param name="innerException">The inner exception.</param>
    protected FargoDomainException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

#endregion Exceptions

#region Entities

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
/// by the application or infrastructure layer (e.g., change tracker).
///
/// <para>
/// <strong>Important:</strong>
/// <see cref="EditedByGuid"/> is expected to be <see langword="null"/>
/// only during the entity creation phase. After the entity is tracked
/// and persisted, this value should always be populated.
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
    /// implementations should typically use
    /// <see cref="System.SystemService.SystemGuid"/>.
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
    /// <see cref="System.SystemService.SystemGuid"/>.
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

#region Actor

/// <summary>
/// Represents an actor responsible for performing operations within the system.
/// </summary>
/// <remarks>
/// An actor abstracts the origin of an action, allowing the domain to treat
/// different initiators uniformly. An actor can be:
/// <list type="bullet">
/// <item>
/// <description>A real authenticated user (<see cref="Fargo.Core.Users.UserActor"/>)</description>
/// </item>
/// <item>
/// <description>The system itself (<see cref="Fargo.Core.System.SystemActor"/>)</description>
/// </item>
/// </list>
///
/// This abstraction enables consistent authorization, auditing, and permission handling
/// across the domain, regardless of whether the action was triggered by a user or internally.
/// </remarks>
public interface IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> that uniquely identifies the actor instance.
    /// </value>
    Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is an administrator; otherwise, <c>false</c>.
    /// </value>
    bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor represents the system.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is the system actor; otherwise, <c>false</c>.
    /// </value>
    bool IsSystem { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    /// <value>
    /// <c>true</c> if the actor is active and allowed to perform actions; otherwise, <c>false</c>.
    /// </value>
    bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="ActionType"/> representing allowed operations.
    /// </value>
    IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    /// <value>
    /// A read-only collection of partition identifiers (<see cref="Guid"/>).
    /// </value>
    IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor has access to the specified partition; otherwise, <c>false</c>.
    /// </returns>
    bool HasPartitionAccess(Guid partitionGuid);

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor has permission for the specified action; otherwise, <c>false</c>.
    /// </returns>
    bool HasActionPermission(ActionType action);
}
/// <summary>
/// Provides a base implementation for <see cref="IActor"/>.
/// </summary>
/// <remarks>
/// The <see cref="Actor"/> class defines common authorization behavior shared by all actor types,
/// including permission and partition access evaluation.
///
/// Concrete implementations (e.g., <see cref="Fargo.Core.Users.UserActor"/> and <see cref="Fargo.Core.System.SystemActor"/>)
/// are responsible for supplying identity and access data.
///
/// Authorization rules follow a hierarchical model:
/// <list type="number">
/// <item><description><b>System actors</b> have unrestricted access to all operations and partitions</description></item>
/// <item><description><b>Administrative actors</b> have unrestricted access within the domain</description></item>
/// <item><description>All other actors are evaluated based on their assigned permissions and partition access</description></item>
/// </list>
/// </remarks>
public abstract class Actor : IActor
{
    /// <summary>
    /// Gets the unique identifier of the actor.
    /// </summary>
    public abstract Guid Guid { get; }

    /// <summary>
    /// Gets a value indicating whether the actor has administrative privileges.
    /// </summary>
    public abstract bool IsAdmin { get; }

    /// <summary>
    /// Gets a value indicating whether the actor represents the system.
    /// </summary>
    public abstract bool IsSystem { get; }

    /// <summary>
    /// Gets a value indicating whether the actor is active.
    /// </summary>
    public abstract bool IsActive { get; }

    /// <summary>
    /// Gets the set of actions the actor is permitted to perform.
    /// </summary>
    public abstract IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>
    /// Gets the set of partitions the actor has access to.
    /// </summary>
    public abstract IReadOnlyCollection<Guid> PartitionAccessesGuids { get; }

    /// <summary>
    /// Determines whether the actor has permission to perform a specific action.
    /// </summary>
    /// <param name="action">The action to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is a system or administrative actor, or if the action
    /// is explicitly granted; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors are always authorized</description></item>
    /// <item><description>Administrative actors are always authorized</description></item>
    /// <item><description>Otherwise, checks <see cref="PermissionActions"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasActionPermission(ActionType action)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        return PermissionActions.Contains(action);
    }

    /// <summary>
    /// Determines whether the actor has access to a specific partition.
    /// </summary>
    /// <param name="partitionGuid">The unique identifier of the partition.</param>
    /// <returns>
    /// <c>true</c> if the actor is a system or administrative actor, or if the partition
    /// is explicitly accessible; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors have unrestricted access</description></item>
    /// <item><description>Administrative actors have unrestricted access</description></item>
    /// <item><description>Otherwise, checks <see cref="PartitionAccessesGuids"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasPartitionAccess(Guid partitionGuid)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        return PartitionAccessesGuids.Contains(partitionGuid);
    }

    /// <summary>
    /// Determines whether the actor has access to a partitioned resource.
    /// </summary>
    /// <param name="partitioned">The partitioned entity to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the actor is a system or administrative actor, if the entity has no
    /// partitions (public), or if at least one partition of the entity is accessible;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Evaluation order:
    /// <list type="number">
    /// <item><description>System actors have unrestricted access</description></item>
    /// <item><description>Administrative actors have unrestricted access</description></item>
    /// <item><description>Entities with no partitions are public and accessible to all authenticated actors</description></item>
    /// <item><description>Otherwise, checks intersection with <see cref="PartitionAccessesGuids"/></description></item>
    /// </list>
    /// </remarks>
    public bool HasAccess(IPartitionedEntity partitioned)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        if (partitioned.Partitions.Count == 0)
        {
            return true;
        }

        return partitioned.Partitions.Any(p => PartitionAccessesGuids.Contains(p.Guid));
    }

    public bool HasAccess(IPartitioned partitioned)
    {
        if (IsSystem || IsAdmin)
        {
            return true;
        }

        if (partitioned.PartitionGuids.Count == 0)
        {
            return true;
        }

        return partitioned.PartitionGuids.Any(p => PartitionAccessesGuids.Contains(p));
    }
}
/// <summary>
/// Service responsible for resolving and constructing <see cref="Actor"/> instances,
/// including <see cref="UserActor"/> and <see cref="SystemActor"/>.
/// </summary>
/// <remarks>
/// This service centralizes the logic for loading users and aggregating their
/// effective partition access based on direct assignments and group memberships.
/// </remarks>

public class ActorService(
    IUserRepository userRepository,
    IPartitionRepository partitionRepository)
{
    /// <summary>
    /// Retrieves an <see cref="Actor"/> instance based on its unique identifier.
    /// </summary>
    /// <param name="actorGuid">The unique identifier of the actor.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="System.SystemActor"/> if the identifier matches the system actor;
    /// a <see cref="UserActor"/> if a corresponding user is found;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// For user actors, this method resolves effective partition access by:
    /// <list type="bullet">
    /// <item><description>Including partitions directly assigned to the user</description></item>
    /// <item><description>Including partitions inherited through user group memberships</description></item>
    /// <item><description>Expanding all collected partitions to include their descendants</description></item>
    /// </list>
    /// This ensures the returned actor contains the complete set of accessible partitions.
    /// </remarks>
    public async Task<Actor?> GetActorByGuid(
        Guid actorGuid,
        CancellationToken cancellationToken = default)
    {
        if (actorGuid == SystemService.SystemGuid)
        {
            var allPartitionAccesses = await partitionRepository.GetDescendantGuids(
                PartitionService.GlobalPartitionGuid,
                includeRoot: true,
                cancellationToken);

            var systemActor = new SystemActor(
                Enum.GetValues<ActionType>(),
                allPartitionAccesses);

            return systemActor;
        }

        var user = await userRepository.GetByGuid(actorGuid, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var parentPartitions = user.PartitionAccesses
            .Select(p => p.PartitionGuid)
            .ToHashSet();

        parentPartitions.UnionWith(
            user.UserGroups
                .Where(group => group.IsActive)
                .SelectMany(g => g.PartitionAccesses)
                .Select(p => p.PartitionGuid)
        );

        var partitionAccess = await partitionRepository
            .GetDescendantGuids(parentPartitions, true, cancellationToken);

        return new UserActor(user, partitionAccess);
    }
}

#endregion Actor

#endregion Entities

#region Value Objects

/// <summary>
/// Represents a validated name value object used across the domain.
///
/// This value object guarantees that a name is always within the
/// allowed length range and is not null, empty, or composed only of whitespace.
/// </summary>
public readonly struct Name : IEquatable<Name>
{
    /// <summary>
    /// Maximum allowed length for a name.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Minimum allowed length for a name.
    /// </summary>
    public const int MinLength = 3;

    private readonly string value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Name"/> struct.
    /// </summary>
    /// <param name="value">The string value representing the name.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or contains only whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value length is outside the allowed range.
    /// </exception>
    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(value));
        }

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Name length must be between {MinLength} and {MaxLength} characters."
            );
        }

        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the name.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Name not initialized.");

    /// <summary>
    /// Creates a new <see cref="Name"/> from a string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>A validated <see cref="Name"/> instance.</returns>
    public static Name FromString(string value)
        => new(value);

    /// <summary>
    /// Creates a new validated <see cref="Name"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A new <see cref="Name"/> instance.</returns>
    public static Name NewName(string value)
        => new(value);

    /// <summary>
    /// Determines whether the current name is equal to another name.
    /// </summary>
    /// <param name="other">The other name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names have the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Name other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current name is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current name.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="Name"/> with the same value;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Name other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current name.
    /// </summary>
    /// <returns>A hash code based on the underlying value.</returns>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are equal.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Name left, Name right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="Name"/> instances are different.
    /// </summary>
    /// <param name="left">The first name to compare.</param>
    /// <param name="right">The second name to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both names are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Name left, Name right)
        => !left.Equals(right);

    /// <summary>
    /// Returns the string representation of the name.
    /// </summary>
    /// <returns>The underlying string value.</returns>
    public override string ToString()
        => Value;

    /// <summary>
    /// Implicitly converts a <see cref="Name"/> to its string representation.
    /// </summary>
    public static implicit operator string(Name name)
        => name.Value;

    /// <summary>
    /// Explicitly converts a string to a <see cref="Name"/>.
    /// </summary>
    public static explicit operator Name(string value)
        => new(value);
}
/// <summary>
/// Represents a validated textual description in the domain.
///
/// This value object allows empty values, but enforces a maximum length.
/// Because it is implemented as a <see langword="struct"/>, the default
/// uninitialized state is considered invalid and is guarded against when accessed.
/// </summary>
public readonly struct Description : IEquatable<Description>
{
    /// <summary>
    /// Minimum allowed length for a description.
    /// </summary>
    public const int MinLength = 0;

    /// <summary>
    /// Maximum allowed length for a description.
    /// </summary>
    public const int MaxLength = 500;

    private readonly string? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Description"/> value object.
    /// </summary>
    /// <param name="value">The textual description.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the description length is outside the allowed range.
    /// </exception>
    public Description(string value)
    {
        Validate(value);
        this.value = value;
    }

    /// <summary>
    /// Gets the underlying string value of the description.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the value object was not properly initialized.
    /// This protects against the default struct state.
    /// </exception>
    public string Value
        => value ?? throw new InvalidOperationException("Description not initialized.");

    /// <summary>
    /// Gets an empty description.
    /// </summary>
    public static Description Empty => new(string.Empty);

    /// <summary>
    /// Creates a new <see cref="Description"/> from the specified string.
    /// </summary>
    /// <param name="value">The textual description.</param>
    /// <returns>A validated <see cref="Description"/> instance.</returns>
    public static Description FromString(string value)
        => new(value);

    /// <summary>
    /// Returns the string representation of the description.
    /// </summary>
    public override string ToString()
        => Value;

    /// <summary>
    /// Determines whether the current description is equal to another.
    /// </summary>
    /// <param name="other">The description to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if both descriptions are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Description other)
        => string.Equals(value, other.value, StringComparison.Ordinal);

    /// <summary>
    /// Determines whether the current description is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified object is a <see cref="Description"/>
    /// and is equal to the current instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object? obj)
        => obj is Description other && Equals(other);

    /// <summary>
    /// Returns a hash code for the description.
    /// </summary>
    public override int GetHashCode()
        => value is null ? 0 : value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Determines whether two descriptions are equal.
    /// </summary>
    /// <param name="left">The first description to compare.</param>
    /// <param name="right">The second description to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both descriptions are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Description left, Description right)
        => left.Equals(right);

    /// <summary>
    /// Determines whether two descriptions are different.
    /// </summary>
    /// <param name="left">The first description to compare.</param>
    /// <param name="right">The second description to compare.</param>
    /// <returns>
    /// <see langword="true"/> if both descriptions are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Description left, Description right)
        => !left.Equals(right);

    /// <summary>
    /// Implicitly converts a <see cref="Description"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="description">The description to convert.</param>
    public static implicit operator string(Description description)
        => description.Value;

    /// <summary>
    /// Explicitly converts a <see cref="string"/> to <see cref="Description"/>.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    public static explicit operator Description(string value)
        => new(value);

    /// <summary>
    /// Validates the specified description value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    private static void Validate(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length < MinLength || value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Description length must be between {MinLength} and {MaxLength} characters.");
        }
    }
}

#endregion Value Objects

#region Permissions

/// <summary>
/// Represents the set of actions that can be authorized in the system.
///
/// Each value defines a specific permission that can be granted
/// to a user through <see cref="Fargo.Core.Users.UserPermission"/>.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Allows creating new articles.
    /// </summary>
    CreateArticle = 0,

    /// <summary>
    /// Allows deleting existing articles.
    /// </summary>
    DeleteArticle = 1,

    /// <summary>
    /// Allows editing existing articles.
    /// </summary>
    EditArticle = 2,

    /// <summary>
    /// Allows creating new items associated with articles.
    /// </summary>
    CreateItem = 3,

    /// <summary>
    /// Allows deleting existing items.
    /// </summary>
    DeleteItem = 4,

    /// <summary>
    /// Allows editing existing items.
    /// </summary>
    EditItem = 5,

    /// <summary>
    /// Allows creating new users.
    /// </summary>
    CreateUser = 6,

    /// <summary>
    /// Allows deleting existing users.
    /// </summary>
    DeleteUser = 7,

    /// <summary>
    /// Allows editing existing users.
    /// </summary>
    EditUser = 8,

    /// <summary>
    /// Allows changing the password of another user without requiring
    /// the current password of that user.
    ///
    /// This permission is typically intended for administrative users
    /// who need to reset another user's password. It does not apply to
    /// a user changing their own password, which should require the
    /// current password.
    /// </summary>
    ChangeOtherUserPassword = 9,

    /// <summary>
    /// Allows creating new user groups.
    /// </summary>
    CreateUserGroup = 10,

    /// <summary>
    /// Allows deleting existing user groups.
    /// </summary>
    DeleteUserGroup = 11,

    /// <summary>
    /// Allows editing existing user groups.
    /// </summary>
    EditUserGroup = 12,

    /// <summary>
    /// Allows modifying the membership of a user group,
    /// including adding or removing users from the group.
    ///
    /// This permission controls operations that change which
    /// users belong to a given <see cref="Fargo.Core.UserGroups.UserGroup"/>.
    /// It does not grant permission to create, delete, or edit the group
    /// itself, which are controlled by <see cref="CreateUserGroup"/>,
    /// <see cref="DeleteUserGroup"/>, and <see cref="EditUserGroup"/>.
    /// </summary>
    ChangeUserGroupMembers = 13,

    /// <summary>
    /// Allows creating partitions.
    /// </summary>
    CreatePartition = 14,

    /// <summary>
    /// Allows deleting existing partitions.
    /// </summary>
    DeletePartition = 15,

    /// <summary>
    /// Allows editing existing partitions.
    /// </summary>
    EditPartition = 16,

    /// <summary>
    /// Allows adding barcodes to articles.
    /// </summary>
    AddBarcode = 17,

    /// <summary>
    /// Allows removing barcodes from articles.
    /// </summary>
    RemoveBarcode = 18,

}

#endregion Permissions
