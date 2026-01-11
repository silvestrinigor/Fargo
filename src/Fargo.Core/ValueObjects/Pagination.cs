namespace Fargo.Domain.ValueObjects
{
    public readonly record struct Pagination
    {
        public const int DefaultPage = 1;
        
        public const int DefaultLimit = 1000;

        public const int MaxLimit = 1000;

        public int Page => pageValue ?? DefaultPage;

        private readonly int? pageValue;

        public int Limit => limitValue ?? DefaultLimit;

        private readonly int? limitValue;

        public int Skip => (Page - 1) * Limit;

        public Pagination(int page, int limit)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);

            if (limit < 1 || limit > MaxLimit)
                throw new ArgumentOutOfRangeException(nameof(limit));

            pageValue = page;
            limitValue = limit;
        }

        public Pagination() : this(DefaultPage, DefaultLimit) { }

        public Pagination(int? page, int? limit) : this(page ?? DefaultPage, limit ?? DefaultLimit) { }
    }
}
