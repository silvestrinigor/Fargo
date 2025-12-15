using Fargo.Domain.Abstracts.Entities;
using Fargo.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class EntityRepository(FargoContext fargoContext) : IEntityRepository
    {
        private readonly FargoContext fargoContext = fargoContext;
        public Task<Named?> GetByGuidAsync(Guid guid)
        {
            return fargoContext.Entities.Where(x => x.Guid == guid).FirstOrDefaultAsync();
        }
    }
}
