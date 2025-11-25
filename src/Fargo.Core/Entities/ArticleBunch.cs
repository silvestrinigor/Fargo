using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Entities
{
    public class ArticleBunch : NamedEntity // TODO:? Change the class name to ArticleUnit?
    {
        public required Article Article { get; init; }
        public Container? Container { get; internal set; }
        public DateTime? ShelfDate => Article.ShelfLife is not null
            ? CreatedAt + Article.ShelfLife.Value
            : null;
        public int Quantity { get; } = 0;
    }
}