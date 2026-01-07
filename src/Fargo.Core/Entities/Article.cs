using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article : Entity
    {
        public override EntityType EntityType => EntityType.Article;

        internal Article() { }

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
