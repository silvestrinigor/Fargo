using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    /// <summary>
    /// Represents a base entity with a unique identifier and creation timestamp. Provides value-based equality
    /// comparison and optional name and description information.
    /// </summary>
    /// <remarks>Entity serves as a foundational type for domain objects that require identity and equality
    /// semantics based on a unique GUID. Two entities are considered equal if they share the same GUID, regardless of
    /// other property values. The NameDescriptionInformation property allows derived entities to associate optional
    /// name and description metadata.</remarks>
    public abstract class Entity : IEquatable<Entity>
    {
        /// <summary>
        /// Initializes a new instance of the Entity class.
        /// </summary>
        public Entity() { }

        /// <summary>
        /// Initializes a new instance of the Entity class with the specified name and description.
        /// </summary>
        /// <param name="name">The name to assign to the entity. Can be null if the entity does not have a name.</param>
        /// <param name="description">The description to assign to the entity. Can be null if the entity does not have a description.</param>
        public Entity(Name? name, Description? description)
        {
            NameDescriptionInformation = new EntityNameDescriptionInformation
            {
                Name = name,
                Description = description
            };
        }

        /// <summary>
        /// Gets the unique identifier for this instance.
        /// </summary>
        /// <remarks>The value is automatically generated when the instance is created and cannot be
        /// changed after initialization.</remarks>
        public Guid Guid
        { 
            get; 
            init; 
        } = Guid.NewGuid();

        /// <summary>
        /// Gets the date and time when the object was created.
        /// </summary>
        public DateTime CreatedAt 
        { 
            get; 
            init; 
        } = DateTime.UtcNow;

        public bool Equals(Entity? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Guid == other.Guid;
        }

        public override bool Equals(object? obj)
            => obj is Entity other && Equals(other);

        public override int GetHashCode()
            => Guid.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right)
            => Equals(left, right);

        public static bool operator !=(Entity? left, Entity? right)
            => !Equals(left, right);

        public EntityNameDescriptionInformation? NameDescriptionInformation
        {
            get;
            protected init;
        }

        public bool HasNameDescriptionInformation
            => NameDescriptionInformation is not null;
    }

    public class EntityNameDescriptionInformation
    {
        public Name? Name
        {
            get;
            set;
        }

        public Description? Description
        {
            get;
            set;
        }
    }
}
