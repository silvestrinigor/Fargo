namespace Fargo.Core.Entities.Abstracts
{
    public abstract class Base : IEquatable<Base>
    {
        public Guid Guid { get; init; } = Guid.NewGuid();

        public override bool Equals(object? obj) =>
            obj is Base other && Guid.Equals(other.Guid);

        public bool Equals(Base? other) =>
            other is not null && (ReferenceEquals(this, other) || Guid.Equals(other.Guid));

        public override int GetHashCode() =>
            Guid.GetHashCode();

        public static bool operator ==(Base? left, Base? right) =>
            ReferenceEquals(left, right) 
            || (left is not null && right is not null && left.Equals(right));
        
        public static bool operator !=(Base? left, Base? right) =>
            !(left == right);
    }
}
