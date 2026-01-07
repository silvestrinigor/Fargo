using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities
{
    public class Item : Entity
    {
        public override EntityType EntityType => EntityType.Item;

        internal Item() 
        { 
        }

        public Guid ArticleGuid
        { 
            get; 
            private init; 
        }

        public required Article Article
        { 
            get; 
            init
            {
                ArticleGuid = value.Guid;
                field = value;
            }
        }
    }
}
