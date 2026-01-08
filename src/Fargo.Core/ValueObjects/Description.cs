namespace Fargo.Domain.ValueObjects
{
    public readonly struct Description(string value) : IEquatable<Description>
    {
        public const int MaxLength = 500;

        public string Value { get; }
            = value.Length > MaxLength
            ? throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.")
            : value;

        public Description() : this(string.Empty) { }

        public static Description NewDescription(string value)
            => new(value);

        public static Description Empty
            => new();

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

        public override string ToString()
            => Value;
    }
}
