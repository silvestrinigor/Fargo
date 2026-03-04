using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
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