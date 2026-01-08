namespace Fargo.Domain.ValueObjects
{
    public readonly struct PasswordHash(string value) : IEquatable<PasswordHash>
    {
        public const int MaxLength = 512;

        public string Value { get; }
            = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Cannot be empty.", nameof(value))
            : value.Length > MaxLength
            ? throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.")
            : value;

        public static PasswordHash NewPasswordHash(string value)
            => new(value);

        public bool Equals(PasswordHash other)
            => Value == other.Value;

        public override bool Equals(object? obj)
            => obj is PasswordHash other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(PasswordHash left, PasswordHash right)
            => left.Equals(right);

        public static bool operator !=(PasswordHash left, PasswordHash right)
            => !left.Equals(right);

        public override string ToString()
            => Value;
    }
}
