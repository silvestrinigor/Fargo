using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Repositories
{
    public interface IModelReadRepository
    {
        Task<Model?> GetByGuidAsync(Guid modelGuid, CancellationToken cancellationToken = default);

        Task<IEnumerable<Model>> GetManyAsync(ModelType? modelType = null, CancellationToken cancellationToken = default);
    }
}
