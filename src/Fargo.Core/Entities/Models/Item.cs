using Fargo.Domain.Entities.Models.Abstracts;
using Fargo.Domain.Enums;

namespace Fargo.Domain.Entities.Models
{
    public class Item : Model
    {
        internal Item() { }

        public override ModelType ModelType
        {
            get;
            protected init;
        } = ModelType.Item;

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
