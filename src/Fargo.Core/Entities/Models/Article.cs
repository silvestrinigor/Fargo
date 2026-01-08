using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities.Models
{
    public class Article : IEntity
    {
        internal Article() { }

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
    }
}
