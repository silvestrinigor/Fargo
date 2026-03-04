namespace Fargo.Domain.ValueObjects
{
    public readonly struct Description
    {
        public Description() : this(string.Empty) { }

        public Description(string value)
        {
            if (value.Length > MaxLength || value.Length < MinLength)
                throw new ArgumentOutOfRangeException(nameof(value), value);

            this.value = value;
        }

        public const int MaxLength = 500;

        public const int MinLength = 0;

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