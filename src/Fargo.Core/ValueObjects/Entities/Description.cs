namespace Fargo.Domain.ValueObjects.Entities
{
    public readonly struct Description(string value) : IEquatable<Description>
    {
        public string Value { get; } 
            = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Description cannot be empty.", nameof(value))
            : value;

        public static Description NewDescription(string value)
            => new(value);

        public bool Equals(Description other)
            => Value == other.Value;

        public override bool Equals(object? obj)
            => obj is Description other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(Description left, Description right)
            => left.Equals(right);

        public static bool operator !=(Description left, Description right)
            => !left.Equals(right);
    }
}
