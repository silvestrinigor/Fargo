using Fargo.Application.Persistence;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoUnitOfWork(FargoContext fagoContext) : IUnitOfWork
    {
        private readonly FargoContext fargoContext = fagoContext;

        public async Task<int> SaveChangesAsync()
            => await fargoContext.SaveChangesAsync();
    }
}
