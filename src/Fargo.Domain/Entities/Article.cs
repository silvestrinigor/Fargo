using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article : AuditedEntity
    {
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

        public HashSet<Partition> Partitions
        {
            get;
            init;
        } = [];
    }
}