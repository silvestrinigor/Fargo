namespace Fargo.Domain.ValueObjects;

/// <summary>
/// Represents pagination parameters used in paginated queries.
/// </summary>
/// <remarks>
/// Pagination combines a <see cref="Page"/> index and a <see cref="Limit"/>
/// to determine which subset of records should be retrieved.
///
/// The <see cref="Skip"/> and <see cref="Take"/> values can be used directly
/// in data queries such as database pagination.
/// </remarks>
public readonly record struct Pagination
{
    /// <summary>
    /// Gets a pagination instance representing the first page
    /// with a limit of 20 items.
    /// </summary>
    /// <remarks>
    /// This property provides a convenient default pagination
    /// configuration commonly used in queries.
    ///
    /// It is equivalent to:
    /// <code>
    /// new Pagination(Page.FirstPage, new Limit(20))
    /// </code>
    /// </remarks>
    public static Pagination First20Pages => new(Page.FirstPage, new Limit(20));

    /// <summary>
    /// Initializes a new instance of <see cref="Pagination"/>.
    /// </summary>
    /// <param name="page">
    /// The page index that determines which page of results to retrieve.
    /// </param>
    /// <param name="limit">
    /// The maximum number of items to return per page.
    /// </param>
    /// <remarks>
    /// Both <paramref name="page"/> and <paramref name="limit"/> must be
    /// valid initialized values. Using the default struct value for either
    /// may result in an <see cref="InvalidOperationException"/> when accessed.
    /// </remarks>
    public Pagination(
        Page page,
        Limit limit)
    {
        Page = page;
        Limit = limit;
    }

    /// <summary>
    /// Gets the page index used for pagination.
    /// </summary>
    public Page Page { get; init; }

    /// <summary>
    /// Gets the maximum number of items returned per page.
    /// </summary>
    public Limit Limit { get; init; }

    /// <summary>
    /// Gets the number of records to skip when executing a paginated query.
    /// </summary>
    /// <remarks>
    /// Calculated as: (<see cref="Page"/> - 1) × <see cref="Limit"/>.
    /// </remarks>
    public int Skip => (Page.Value - 1) * Limit.Value;

    /// <summary>
    /// Gets the maximum number of records to retrieve.
    /// </summary>
    /// <remarks>
    /// This value is equivalent to <see cref="Limit"/>.
    /// </remarks>
    public int Take => Limit.Value;
}
