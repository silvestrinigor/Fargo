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

        public async Task<IEnumerable<Model>> GetManyAsync(ModelType? modelType = null, CancellationToken cancellationToken = default)
        {
            var query = context.Models.AsQueryable();

            if (modelType != null)
            {
                query = query.Where(x => x.ModelType == modelType);
            }

            return await query
                .ToListAsync(cancellationToken);
        }
    }
}
