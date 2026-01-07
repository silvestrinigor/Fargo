using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ModelSingleQuery(
        Guid ModelGuid
        ) : IQuery<ModelDto?>;

    public sealed class ModelSingleQueryHandler(IModelReadRepository repository) : IQueryHandlerAsync<ModelSingleQuery, ModelDto?>
    {
        private readonly IModelReadRepository repository = repository;

        public async Task<ModelDto?> HandleAsync(ModelSingleQuery query, CancellationToken cancellationToken = default)
        {
            var models = await repository.GetByGuidAsync(query.ModelGuid, cancellationToken);

            return models?.ToDto();
        }
    }
}
