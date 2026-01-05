using Fargo.Domain.Enums;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities
{
    public class Article : IEntityTyped
    {
        internal Article()
        {
            EntityType = EntityType.Article;
        }

        public Guid Guid
        {
            get;
            init;
        } = Guid.NewGuid();

        public EntityType EntityType 
        {
            get; 
        }

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

        public DateTime CreatedAt
        {
            get;
            init;
        } = DateTime.UtcNow;
    }
}
