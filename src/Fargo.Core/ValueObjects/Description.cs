namespace Fargo.Domain.ValueObjects
{
    public readonly struct Description : IStringValueObject<Description>
    {
        public Description() : this(string.Empty) { }

        public Description(string value)
        {
            if (value.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot exceed {MaxLength} characters.");

            this.value = value;
        }

        public const int MaxLength = 500;

        public string Value => value is not null ? value : string.Empty;

        private readonly string? value;

        public static Description Empty
            => new(string.Empty);

        public static Description FromString(string value)
            => new(value);

        public static implicit operator string(Description description) => description.Value;

        public static explicit operator Description(string value) => new(value);
    }
}
