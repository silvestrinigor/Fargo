namespace Fargo.Domain.Entities
{
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

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
    }
}
