using Fargo.Infrastructure.Contexts;
using Fargo.Application.Contracts.UnitOfWork;

namespace Fargo.Infrastructure.UnitOfWork;

public class FargoUnitOfWork(FargoContext fagoContext) : IUnitOfWork
{
    public async Task SaveChangesAsync()
    {
        await fagoContext.SaveChangesAsync();
    }
}
