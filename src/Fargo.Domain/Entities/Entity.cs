namespace Fargo.Domain.Entities
{
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
    public abstract class Entity
    {
        /// <summary>
        /// Gets the unique identifier of the entity.
        ///
        /// The identifier is generated automatically using <see cref="Guid.NewGuid"/>
        /// when the entity instance is created.
        /// </summary>
        public Guid Guid
        {
            get;
            init;
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
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            if (Guid == Guid.Empty || other.Guid == Guid.Empty)
                return false;

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
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
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
}