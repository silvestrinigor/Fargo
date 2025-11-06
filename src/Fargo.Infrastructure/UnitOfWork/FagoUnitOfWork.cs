using Fargo.Infrastructure.Contexts;
using Fargo.Application.Contracts.UnitOfWork;

namespace Fargo.Infrastructure.UnitOfWork;

public class FagoUnitOfWork(FagoContext fagoContext) : IUnitOfWork
{
    public async Task SaveChangesAsync()
    {
        await fagoContext.SaveChangesAsync();
    }
}
