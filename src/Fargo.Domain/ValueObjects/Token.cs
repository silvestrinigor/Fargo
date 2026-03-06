namespace Fargo.Domain.ValueObjects
{
    public readonly struct Token
    {
        public Token(string value)
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

        public static Token FromString(string value) => new(value);

        public static implicit operator string(Token token) => token.Value;

        public static explicit operator Token(string value) => new(value);
    }
}