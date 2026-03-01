namespace Fargo.Domain.ValueObjects
{
    public readonly struct Nameid
    {
        public Nameid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Cannot be empty.", nameof(value));

            if (value.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.");

            this.value = value;
        }

        public const int MaxLength = 100;

        public string Value
            => value != string.Empty ? value : throw new ArgumentException("Cannot be empty.", nameof(Value));

        private readonly string value;

        public static Nameid FromString(string value)
            => new(value);

        public static implicit operator string(Nameid name) => name.Value;

        public static explicit operator Nameid(string value) => new(value);

        public static Nameid NewNameid(string value)
            => new (value);
    }
}