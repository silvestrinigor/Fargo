using Fargo.Application.Common;

namespace Fargo.Application.Requests.Queries.TreeQueries
{
    public sealed record PartitionsTreeQuery(
            int? Label = null,
            Pagination? Pagination = null
            ) : IQuery<Guid?>;
}