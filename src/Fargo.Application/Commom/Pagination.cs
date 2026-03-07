namespace Fargo.Application.Commom
{
    /// <summary>
    /// Represents pagination parameters used in paginated queries.
    /// </summary>
    /// <remarks>
    /// Pagination combines a page index and a limit to determine
    /// which subset of records should be retrieved.
    /// </remarks>
    public readonly record struct Pagination
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Pagination"/>.
        /// </summary>
        /// <param name="page">
        /// The page index. Defaults to <see cref="Page.DefaultValue"/>.
        /// </param>
        /// <param name="limit">
        /// The maximum number of items per page.
        /// Defaults to <see cref="Limit.DefaultValue"/>.
        /// </param>
        public Pagination(
                Page page = default,
                Limit limit = default
                )
        {
            Page = page;
            Limit = limit;
        }

        /// <summary>
        /// Gets the page index.
        /// </summary>
        public Page Page { get; init; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        public Limit Limit { get; init; }

        /// <summary>
        /// Gets the number of records to skip when executing a paginated query.
        /// </summary>
        public int Skip => (Page.Value - 1) * Limit.Value;

        /// <summary>
        /// Gets the maximum number of records to retrieve.
        /// </summary>
        public int Take => Limit.Value;
    }
}