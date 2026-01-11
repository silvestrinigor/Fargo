namespace Fargo.Application.Commom
{
    public readonly struct Page
    {
        public Page(int value)
        {
            if (value < MinValue || value > MaxValue)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    $"Must be between {MinValue} and {MaxValue}");

            this.value = value;
        }

        public const int MinValue = 1;

        public const int MaxValue = int.MaxValue;

        public const int DefaultValue = MinValue;

        public int Value => value == 0 ? DefaultValue : value;

        private readonly int value;

        public static implicit operator int(Page page) => page.Value;

        public static explicit operator Page(int value) => new(value);
    }
}
