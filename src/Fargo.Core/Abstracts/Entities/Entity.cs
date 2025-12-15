namespace Fargo.Domain.Abstracts.Entities
{
    public abstract class Entity : IEquatable<Entity>
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

        public override bool Equals(object? obj) =>
            obj is Entity other && Guid.Equals(other.Guid);

        public bool Equals(Entity? other) =>
            other is not null && (ReferenceEquals(this, other) || Guid.Equals(other.Guid));

        public override int GetHashCode() =>
            Guid.GetHashCode();

        public static bool operator ==(Entity? left, Entity? right) =>
            ReferenceEquals(left, right)
            || (left is not null && right is not null && left.Equals(right));

        public static bool operator !=(Entity? left, Entity? right) =>
            !(left == right);
    }
}
