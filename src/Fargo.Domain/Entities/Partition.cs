using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Partition : AuditedEntity
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
    }
}