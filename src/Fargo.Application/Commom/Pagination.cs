namespace Fargo.Application.Commom
{
    public readonly record struct Pagination
    {
        public Pagination(
                Page page = default,
                Limit limit = default
                )
        {
            Page = page;
            Limit = limit;
        }

        public Page Page { get; init; }

        public Limit Limit { get; init; }

        public int Skip => (Page - 1) * Limit;
    }
}