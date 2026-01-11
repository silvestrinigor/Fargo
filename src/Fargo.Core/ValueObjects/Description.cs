namespace Fargo.Domain.ValueObjects
{
    public readonly struct Description : IEquatable<Description>
    {
        public const string DefaultValue = "";

        public const int MaxLength = 500;

        public string Value => descriptionValue ?? DefaultValue;

        private readonly string descriptionValue;

        public Description() : this(string.Empty) { }

        public Description(string value)
        {
            descriptionValue = value.Length > MaxLength
            ? throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.")
            : value;
        }

        public static Description NewDescription(string value)
            => new(value);

        public static Description Empty
            => new(string.Empty);

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
