namespace Fargo.Domain.ValueObjects
{
    public readonly struct TokenHash
    {
        public TokenHash(string value)
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
            : throw new InvalidOperationException("Value must be set.");

        private readonly string value;

        public static TokenHash FromString(string value) => new(value);

        public static implicit operator string(TokenHash tokenHash) => tokenHash.Value;

        public static explicit operator TokenHash(string value) => new(value);
    }
}