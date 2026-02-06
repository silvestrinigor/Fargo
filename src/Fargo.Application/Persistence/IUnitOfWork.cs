namespace Fargo.Application.Persistence
{
    public interface IUnitOfWork
    {
        Task<int> SaveChanges(CancellationToken cancellationToken = default);
    }
}