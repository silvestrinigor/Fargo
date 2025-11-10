using Fargo.Application.Contracts.Persistence;

namespace Fargo.Infrastructure.Persistence;

public class FargoUnitOfWork(FargoContext fagoContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync()
        => await fagoContext.SaveChangesAsync();
}
