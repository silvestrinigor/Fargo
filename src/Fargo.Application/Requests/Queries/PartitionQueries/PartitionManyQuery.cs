using Fargo.Application.Dtos;
using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;
using UnitsNet.Units;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionManyQuery(
        DateTime? AtDateTime,
        PaginationDto Pagination
        ) : IQuery<IEnumerable<PartitionDto>>;

    public sealed class PartitionManyQueryHandler(IPartitionReadRepository repository) : IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionDto>>
    {
        public readonly IPartitionReadRepository repository = repository;

        public async Task<IEnumerable<PartitionDto>> HandleAsync(PartitionManyQuery query, CancellationToken cancellationToken = default)
        {
            var partitions = await repository.GetManyAsync(
                query.AtDateTime,
                query.Pagination.Skip,
                query.Pagination.Limit,
                cancellationToken);

            return partitions.Select(x => x.ToDto());
        }
    }
}
