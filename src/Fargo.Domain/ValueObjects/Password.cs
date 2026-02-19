namespace Fargo.Domain.ValueObjects
{
    public readonly struct Password : IStringValueObject<Password>
    {
        public Password(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Cannot be empty.", nameof(value));

            if (value.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value), $"Cannot exceed {MaxLength} characters.");

            this.value = value;
        }

        public const int MaxLength = 512;

        public string Value
            => value != string.Empty ? value : throw new InvalidOperationException("Password value must be set.");

        private readonly string value;

        public static Password FromString(string value)
            => new(value);

        public static implicit operator string(Password password) => password.Value;

        public static explicit operator Password(string value) => new(value);
    }
}
