namespace Fargo.Domain.ValueObjects
{
    public readonly struct Password(string value) : IEquatable<Password>
    {
        public const int MaxLength = 512;

        public string Value { get; }
            = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Cannot be empty.", nameof(value))
            : value.Length > MaxLength
            ? throw new ArgumentOutOfRangeException(nameof(value), $"Cannot exceed {MaxLength} characters.")
            : value;

        public static Password NewPassword(string value)
            => new(value);

        public bool Equals(Password other)
            => Value == other.Value;

        public override bool Equals(object? obj)
            => obj is Password other && Equals(other);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(Password left, Password right)
            => left.Equals(right);

        public static bool operator !=(Password left, Password right)
            => !left.Equals(right);

        public override string ToString()
            => Value;
    }
}
