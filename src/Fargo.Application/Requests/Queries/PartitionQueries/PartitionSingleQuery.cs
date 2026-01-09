using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries.PartitionQueries
{
    public sealed record PartitionSingleQuery(
        Guid PartitionGuid,
        DateTime? AtDateTime
        ) : IQuery<PartitionDto?>;

    public sealed class PartitionSingleQueryHandler(IPartitionReadRepository repository) : IQueryHandlerAsync<PartitionSingleQuery, PartitionDto?>
    {
        private readonly IPartitionReadRepository repository = repository;

        public async Task<PartitionDto?> HandleAsync(PartitionSingleQuery query, CancellationToken cancellationToken = default)
            => (await repository.GetByGuidAsync(query.PartitionGuid, query.AtDateTime, cancellationToken))?.ToDto();
    }
}
