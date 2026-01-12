namespace Fargo.Domain.ValueObjects
{
    public readonly struct Name : IStringValueObject<Name>
    {
        public Name(string value)
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

        public static Name FromString(string value)
            => new(value);

        public static implicit operator string(Name name) => name.Value;

        public static explicit operator Name(string value) => new(value);
    }
}
