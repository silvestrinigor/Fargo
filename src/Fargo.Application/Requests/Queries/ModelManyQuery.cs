using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ModelManyQuery(
        ModelType? ModelType
        ) : IQuery<IEnumerable<ModelDto>>;

    public sealed class ModelManyQueryHandler(IModelReadRepository repository) : IQueryHandlerAsync<ModelManyQuery, IEnumerable<ModelDto>>
    {
        private readonly IModelReadRepository repository = repository;

        public async Task<IEnumerable<ModelDto>> HandleAsync(ModelManyQuery query, CancellationToken cancellationToken = default)
        {
            var models = await repository.GetManyAsync(query.ModelType, cancellationToken);

            return models.Select(x => x.ToDto());
        }
    }
}
