using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article
    {
        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public Name Name
        {
            get;
            private set;
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

        public PartitionCollection Partitions
        {
            get;
            init;
        } = [];
    }
}