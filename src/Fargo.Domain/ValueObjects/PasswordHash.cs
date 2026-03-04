namespace Fargo.Domain.ValueObjects
{
    public readonly struct PasswordHash
    {
        public PasswordHash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Cannot be empty.", nameof(value));

            if (value.Length > MaxLength || value.Length < MinLength)
                throw new ArgumentOutOfRangeException(nameof(value), value.Length.ToString());

            this.value = value;
        }

        public const int MaxLength = 512;

        public const int MinLength = 50;

        public string Value
            => value != string.Empty
            ? value
            : throw new InvalidOperationException("Password hash value must be set.");

        private readonly string value;

        public static PasswordHash FromString(string value) => new(value);

        public static implicit operator string(PasswordHash passwordHash) => passwordHash.Value;

        public static explicit operator PasswordHash(string value) => new(value);
    }
}