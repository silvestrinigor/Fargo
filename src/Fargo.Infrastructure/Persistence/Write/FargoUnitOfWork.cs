using Fargo.Application.Persistence;

namespace Fargo.Infrastructure.Persistence.Write
{
    public class FargoUnitOfWork(FargoWriteDbContext fagoContext) : IUnitOfWork
    {
        private readonly FargoWriteDbContext fargoContext = fagoContext;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
            => await fargoContext.SaveChangesAsync(cancellationToken);
    }
}
