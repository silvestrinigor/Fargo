namespace Fargo.Application.Requests.Queries.TreeQueries
{
    public sealed record PartitionTreeQuery(

            ) : IQuery<int>;

    public sealed record PartitionTreeQueryHandler(

            ) : IQueryHandler<PartitionTreeQuery, int>
    {
        public Task<int> Handle(PartitionTreeQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}