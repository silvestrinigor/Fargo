using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities
{
    public class ArticleItem : Entity
    {
        public ArticleItem() : base() { }
        public ArticleItem(string name) : base(name) { }
        public ArticleItem(string name, Guid guid) : base(name, guid) { }
        public required Article Article { get; init; }
    }
}