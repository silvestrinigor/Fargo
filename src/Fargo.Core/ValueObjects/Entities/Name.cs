namespace Fargo.Domain.ValueObjects.Entities
{
    public readonly struct Name
    {
        public string Value { get; }

        public Name(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.", nameof(value));

            Value = value;
        }

        public static Name NewName(string value)
            => new(value);
    }
}
