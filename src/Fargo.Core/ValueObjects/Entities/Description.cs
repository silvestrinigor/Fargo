namespace Fargo.Domain.ValueObjects.Entities
{
    public readonly struct Description
    {
        public string Value { get; }

        public Description(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Description cannot be empty.", nameof(value));

            Value = value;
        }

        public static Description NewDescription(string value)
            => new(value);
    }
}
