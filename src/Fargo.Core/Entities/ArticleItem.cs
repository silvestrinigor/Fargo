using Fargo.Core.Entities.Abstracts;

namespace Fargo.Core.Entities
{
    public class ArticleItem : Entity
    {
        public required Article Article { get; init; }
    }
}