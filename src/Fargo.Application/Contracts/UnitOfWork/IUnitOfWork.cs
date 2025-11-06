namespace Fargo.Application.Contracts.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
