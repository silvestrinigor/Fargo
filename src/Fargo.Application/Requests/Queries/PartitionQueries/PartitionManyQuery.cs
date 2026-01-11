using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories.PartitionRepositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionManyQuery(
        DateTime? AtDateTime = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<PartitionDto>>;

    public sealed class PartitionManyQueryHandler(IPartitionReadRepository repository) : IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionDto>>
    {
        public readonly IPartitionReadRepository repository = repository;

        public async Task<IEnumerable<PartitionDto>> HandleAsync(PartitionManyQuery query, CancellationToken cancellationToken = default)
        {
            var partitions = await repository.GetManyAsync(
                query.AtDateTime,
                query.Pagination,
                cancellationToken);

            return partitions.Select(x => x.ToDto());
        }
    }
}
