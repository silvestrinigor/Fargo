using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ModelReadRepository(FargoContext context) : IModelReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<Model?> GetByGuidAsync(Guid modelGuid, CancellationToken cancellationToken = default)
        {
            return await context.Models
                .AsNoTracking()
                .Where(x => x.Guid == modelGuid)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public Task<IEnumerable<Model>> GetManyAsync(ModelType? modelType = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
