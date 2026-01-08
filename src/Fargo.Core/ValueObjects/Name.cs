namespace Fargo.Domain.ValueObjects
{
    public readonly struct Name(string value) : IEquatable<Name>
    {
        public const int MaxLength = 100;

        public string Value { get; }
            = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Cannot be empty.", nameof(value))
            : value.Length > MaxLength
            ? throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.")
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

        public override string ToString()
            => Value;
    }
}
