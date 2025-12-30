namespace Fargo.Domain.ValueObjects
{
    public readonly struct Name(string value) : IEquatable<Name>
    {
        public string Value { get; }
            = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Name cannot be empty.", nameof(value))
            : value;

        public static Name NewName(string value)
            => new(value);
        
        public bool Equals(Name other)
            => Value == other.Value;

        public override bool Equals(object? obj)
            => obj is Name other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(Name left, Name right)
            => left.Equals(right);

        public static bool operator !=(Name left, Name right)
            => !left.Equals(right);
    }
}
