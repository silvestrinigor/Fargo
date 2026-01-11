namespace Fargo.Application.Commom
{
    public readonly record struct Limit
    {
        public Limit(int value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Must be between {MinValue} and {MaxValue}");

            this.value = value;
        }

        public const int MinValue = 1;

        public const int MaxValue = 1000;

        public const int DefaultValue = 20;

        public int Value => value == 0 ? DefaultValue : value;

        private readonly int value;

        public static implicit operator int(Limit limit) => limit.Value;

        public static explicit operator Limit(int value) => new(value);
    }
}
