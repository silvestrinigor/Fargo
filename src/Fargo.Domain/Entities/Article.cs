using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article
        : IEntity, IEntityByGuid, IEntityTemporal, IEntityPartitioned
    {
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public required Name Name
        {
            get;
            set;
        }

        public Description Description
        {
            get;
            set;
        } = Description.Empty;

        public bool IsContainer
        {
            get;
            init;
        } = false;

        private readonly HashSet<Partition> partitions = [];

        public IReadOnlyCollection<Partition> Partitions => partitions;
    }
}