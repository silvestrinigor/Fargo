namespace Fargo.Domain.Entities
{
    public interface IEntityPartitioned : IEntity
    {
        public IReadOnlyCollection<Partition> Partitions
        {
            get;
        }
    }
}