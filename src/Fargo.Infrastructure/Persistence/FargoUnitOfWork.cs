using Fargo.Application.Persistence;

namespace Fargo.Infrastructure.Persistence
{
    public class FargoUnitOfWork(FargoWriteDbContext fargoContext) : IUnitOfWork
    {
        private readonly FargoWriteDbContext fargoContext = fargoContext;

        public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
            => await fargoContext.SaveChangesAsync(cancellationToken);
    }
}