using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ModelRepository(FargoContext context) : IModelRepository
    {
        private readonly FargoContext context = context;

        public void Add(Model model)
        {
            context.Models.Add(model);
        }

        public async Task<Model?> GetByGuidAsync(Guid guid)
        {
            return await context.Models
                .Where(model => model.Guid == guid)
                .FirstOrDefaultAsync();
        }

        public void Remove(Model model)
        {
            context.Models.Remove(model);
        }
    }
}
