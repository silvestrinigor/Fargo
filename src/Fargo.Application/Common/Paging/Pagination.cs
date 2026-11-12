namespace Fargo.Application.Common.Paging;

public readonly record struct Pagination
{
    public Page Page { get; init; }

    public Limit Limit { get; init; }

    public Pagination()
    {
        Page = Page.FirstPage;

        Limit = Limit.MaxLimit;
    }

    public Pagination(Page page, Limit limit)
    {
        Page = page;
        Limit = limit;
    }

    public int Skip => (Page.Value - 1) * Limit.Value;

    public int Take => Limit.Value;
}
