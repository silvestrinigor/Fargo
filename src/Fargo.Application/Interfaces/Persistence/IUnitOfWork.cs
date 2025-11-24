namespace Fargo.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
